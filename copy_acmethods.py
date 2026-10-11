#!/usr/bin/env python3
"""
Copy HandleExecuteACMethod and GetPropsToObserveForIsEnabled from V5 to V4.

Scans V5 for C# files containing both methods, then copies/replaces them
into the corresponding V4 files. Creates a log file noting any inconsistencies.
"""

import os
import sys
import shutil
from datetime import datetime

# ── Constants ────────────────────────────────────────────────────────────────

METHOD_NAMES = ("HandleExecuteACMethod", "GetPropsToObserveForIsEnabled")

SIGNATURE_PREFIXES = {
    "HandleExecuteACMethod": "HandleExecuteACMethod(out object result,",
    "GetPropsToObserveForIsEnabled": "GetPropsToObserveForIsEnabled(string acMethodName)",
}

DEFAULT_V4_DIR = "/home/damir/SHARED/Devel/iPlusGit/V4"
DEFAULT_V5_DIR = "/home/damir/SHARED/Devel/iPlusGit/V5"

# ── Helpers ──────────────────────────────────────────────────────────────────

def find_method_start(lines, method_name):
    """Return the 0-based line index of *method_name* signature, or None."""
    prefix = SIGNATURE_PREFIXES[method_name]
    for i, line in enumerate(lines):
        stripped = line.strip()
        if prefix in stripped:
            return i
    return None


def extract_method_body(lines, start_idx):
    """Given the signature line index, return (body_lines, end_exclusive_idx).

    Counts braces to find the closing '}'; end_exclusive is one past the
    closing-brace line.
    """
    brace_count = 0
    started = False          # True once we've seen the first '{'
    for i in range(start_idx, len(lines)):
        for ch in lines[i]:
            if ch == '{':
                brace_count += 1
                started = True
            elif ch == '}':
                brace_count -= 1
        if started and brace_count == 0:
            return lines[start_idx:i + 1], i + 1
    # fell off – shouldn't happen with valid C#
    raise RuntimeError(f"Incomplete method body starting at line {start_idx}")


def find_region_end(lines, region_marker="#endregion"):
    """Return the 0-based index of the FIRST occurrence of *region_marker*,
    or None.  We want the one closest to our methods (usually the
    Execute-Helper-Handlers close)."""
    for i, line in enumerate(lines):
        if line.strip().startswith(region_marker):
            return i
    return None


def find_insertion_point(lines):
    """Determine where to insert new methods.

    Priority:
      1. Just before '#endregion' that closes Execute-Helper-Handlers
         (we look for ANY '#endregion' – in practice the first one after
          the methods region is the right one).
      2. End-of-file fallback.
    """
    idx = find_region_end(lines, "#endregion")
    if idx is not None:
        return idx
    return len(lines)  # append at end


# ── Extraction ───────────────────────────────────────────────────────────────

def extract_methods_from_v5(content):
    """Return dict {method_name: body_text} for methods found in *content*.

    Raises ValueError if a method is missing.
    """
    lines = content.split('\n')
    result = {}

    for mname in METHOD_NAMES:
        start = find_method_start(lines, mname)
        if start is None:
            raise ValueError(f"{mname} not found")
        body_lines, _ = extract_method_body(lines, start)
        result[mname] = '\n'.join(body_lines)

    return result


# ── Migration ────────────────────────────────────────────────────────────────

def migrate_file(v5_content, v4_content, log_entries):
    """Apply V5 methods to V4 content. Return (modified_content, status).

    Status is one of: replaced, inserted_both, inserted_one, none_needed.
    """
    v4_lines = v4_content.split('\n')

    # Which methods exist in V4?
    h_exists = find_method_start(v4_lines, "HandleExecuteACMethod") is not None
    g_exists = find_method_start(v4_lines, "GetPropsToObserveForIsEnabled") is not None

    if h_exists and g_exists:
        # Both exist → replace both
        return replace_both(v4_lines, v5_content, log_entries)

    if not h_exists and not g_exists:
        # Neither exists → insert both
        return insert_both(v4_lines, v5_content, log_entries)

    # Exactly one exists → replace that one + insert the other
    if h_exists:
        return replace_handle_insert_props(v4_lines, v5_content, log_entries)
    else:
        return replace_props_insert_handle(v4_lines, v5_content, log_entries)


# --- Replacement helpers ---------------------------------------------------

def replace_method(lines, method_name, new_body):
    """Replace *method_name* in-place. Return new lines list."""
    start = find_method_start(lines, method_name)
    assert start is not None
    _, end_excl = extract_method_body(lines, start)
    new_lines = new_body.split('\n')
    return lines[:start] + new_lines + lines[end_excl:]


def replace_both(v4_lines, v5_content, log_entries=None):
    """Both methods exist in V4 – replace both."""
    methods = extract_methods_from_v5(v5_content)
    # Replace from bottom-up so indices stay valid.
    h_start = find_method_start(v4_lines, "HandleExecuteACMethod")
    g_start = find_method_start(v4_lines, "GetPropsToObserveForIsEnabled")
    assert h_start is not None and g_start is not None

    if g_start > h_start:
        # GetProps is after Handle – replace GetProps first
        result = replace_method(list(v4_lines), "GetPropsToObserveForIsEnabled",
                               methods["GetPropsToObserveForIsEnabled"])
        result = replace_method(result, "HandleExecuteACMethod",
                                methods["HandleExecuteACMethod"])
    else:
        # Unusual ordering – replace Handle first
        result = replace_method(list(v4_lines), "HandleExecuteACMethod",
                                methods["HandleExecuteACMethod"])
        result = replace_method(result, "GetPropsToObserveForIsEnabled",
                               methods["GetPropsToObserveForIsEnabled"])

    return ('\n'.join(result), "replaced")


# --- Insertion helpers ----------------------------------------------------

def insert_both(v4_lines, v5_content, log_entries=None):
    """Neither method exists – insert both at the computed point."""
    methods = extract_methods_from_v5(v5_content)
    ins_pt = find_insertion_point(v4_lines)

    combined = methods["HandleExecuteACMethod"] + "\n\n" + methods["GetPropsToObserveForIsEnabled"]
    new_block = combined.split('\n')

    result = v4_lines[:ins_pt] + new_block + [''] + v4_lines[ins_pt:]
    return ('\n'.join(result), "inserted_both")


def replace_handle_insert_props(v4_lines, v5_content, log_entries=None):
    """HandleExecuteACMethod exists, GetPropsToObserveForIsEnabled does not."""
    methods = extract_methods_from_v5(v5_content)

    # Replace HandleExecuteACMethod
    result = replace_method(list(v4_lines), "HandleExecuteACMethod",
                           methods["HandleExecuteACMethod"])

    # Find end of (now-replaced) HandleExecuteACMethod and insert GetProps after
    h_start = find_method_start(result, "HandleExecuteACMethod")
    _, h_end = extract_method_body(result, h_start)

    props_lines = methods["GetPropsToObserveForIsEnabled"].split('\n')
    result = result[:h_end] + ['', '\n'.join(props_lines)] + result[h_end:]

    return ('\n'.join(result), "inserted_GetProps_after_Handle")


def replace_props_insert_handle(v4_lines, v5_content, log_entries=None):
    """GetPropsToObserveForIsEnabled exists, HandleExecuteACMethod does not."""
    methods = extract_methods_from_v5(v5_content)

    # Replace GetPropsToObserveForIsEnabled
    result = replace_method(list(v4_lines), "GetPropsToObserveForIsEnabled",
                           methods["GetPropsToObserveForIsEnabled"])

    # Find GetProps and insert Handle BEFORE it (so Handle comes first)
    g_start = find_method_start(result, "GetPropsToObserveForIsEnabled")

    handle_lines = methods["HandleExecuteACMethod"].split('\n')
    result = result[:g_start] + ['', '\n'.join(handle_lines)] + result[g_start:]

    return ('\n'.join(result), "inserted_Handle_before_GetProps")


# ── Main ─────────────────────────────────────────────────────────────────────

def process_directories(v4_base, v5_base):
    """Scan V5, migrate to V4, return stats tuple."""
    # Collect V5 files that have BOTH methods
    v5_candidates = []
    for dirpath, _, filenames in os.walk(v5_base):
        for fn in filenames:
            if not fn.endswith('.cs'):
                continue
            fpath = os.path.join(dirpath, fn)
            rel = os.path.relpath(fpath, v5_base)
            try:
                with open(fpath, 'r', encoding='utf-8', errors='replace') as fh:
                    content = fh.read()
                extract_methods_from_v5(content)  # raises if missing
                v5_candidates.append((rel, content))
            except ValueError:
                pass  # missing one of the methods
            except Exception as exc:
                print(f"[WARN] Could not read {rel}: {exc}", file=sys.stderr)

    print(f"\nFound {len(v5_candidates)} V5 files with both methods.")

    # Process each candidate
    log_entries = []
    migrated = 0
    skipped = 0
    manual_review = 0

    for rel, v5_content in sorted(v5_candidates):
        v4_rel = rel.replace("/iPlus/", "/iPlusMES/", 1) if "/iPlus/" in rel else rel
        # Map V5 subdir → V4 subdir
        v4_fpath = os.path.normpath(os.path.join(v4_base, rel))

        if not os.path.isfile(v4_fpath):
            skipped += 1
            continue

        try:
            with open(v4_fpath, 'r', encoding='utf-8', errors='replace') as fh:
                v4_content = fh.read()
        except Exception as exc:
            print(f"[ERROR] Cannot read {v4_fpath}: {exc}", file=sys.stderr)
            skipped += 1
            continue

        new_content, status = migrate_file(v5_content, v4_content, log_entries)

        if status == "none_needed":
            continue

        # Write updated V4 file
        try:
            with open(v4_fpath, 'w', encoding='utf-8') as fh:
                fh.write(new_content)
            migrated += 1
            print(f"  [{status}] {rel}")
        except Exception as exc:
            print(f"[ERROR] Cannot write {v4_fpath}: {exc}", file=sys.stderr)
            skipped += 1

    # Check V5 inconsistency: GetProps not immediately after Handle
    for rel, v5_content in sorted(v5_candidates):
        lines = v5_content.split('\n')
        h_start = find_method_start(lines, "HandleExecuteACMethod")
        g_start = find_method_start(lines, "GetPropsToObserveForIsEnabled")
        if h_start is None or g_start is None:
            continue
        _, h_end = extract_method_body(lines, h_start)
        # GetProps should start within a few lines after Handle ends
        gap = g_start - h_end
        if gap > 5:  # more than a couple blank lines / comments
            ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            log_entries.append(
                f"[{ts}] INCONSISTENT ORDERING in V5: {rel}\n"
                f"       HandleExecuteACMethod ends at line {h_end+1}, "
                f"GetPropsToObserveForIsEnabled starts at line {g_start+1} "
                f"(gap={gap}). Manual review needed.\n"
            )
            manual_review += 1

    return migrated, skipped, manual_review, log_entries


def write_log(log_entries, log_path):
    with open(log_path, 'w', encoding='utf-8') as fh:
        fh.write("=" * 72 + "\n")
        fh.write("AC Method Copy Log\n")
        fh.write(f"Generated: {datetime.now().isoformat()}\n")
        fh.write("=" * 72 + "\n\n")
        if log_entries:
            for entry in log_entries:
                fh.write(entry + "\n")
        else:
            fh.write("(no inconsistencies detected)\n")


# ── Entry point ──────────────────────────────────────────────────────────────

if __name__ == "__main__":
    import argparse

    ap = argparse.ArgumentParser(
        description="Copy HandleExecuteACMethod & GetPropsToObserveForIsEnabled "
                    "from V5 to V4."
    )
    ap.add_argument("--v4-dir", default=DEFAULT_V4_DIR,
                    help="Root V4 directory (default: %(default)s)")
    ap.add_argument("--v5-dir", default=DEFAULT_V5_DIR,
                    help="Root V5 directory (default: %(default)s)")
    args = ap.parse_args()

    v4_base = args.v4_dir.rstrip('/')
    v5_base = args.v5_dir.rstrip('/')

    print(f"V4 base : {v4_base}")
    print(f"V5 base : {v5_base}")

    migrated, skipped, manual_review, log_entries = process_directories(v4_base, v5_base)

    log_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_path = os.path.join(os.getcwd(), f"ac_method_copy_{log_ts}.log")
    write_log(log_entries, log_path)

    print("\n" + "=" * 50)
    print(f"Migrated     : {migrated}")
    print(f"Skipped       : {skipped}")
    print(f"Manual review : {manual_review}")
    print(f"Log file      : {log_path}")
    print("=" * 50)
