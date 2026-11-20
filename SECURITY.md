# Security Policy

## Supported versions

Only the latest release of the `main` branch is supported with security
fixes.

## Reporting a vulnerability

Please report vulnerabilities privately via GitHub's
**private vulnerability reporting** (Security tab → Report a vulnerability),
or by email to the address below if that is not possible.

https://github.com/iplus-framework/iPlus/security/advisories/new

**Contact:** damir.lisak@live.de (project maintainer)

Please include a description of the issue, the affected component
(e.g. `gip.core.webservices`, `PASqlTunnelHost`, `gip.core.licensing`),
the steps or code needed to reproduce it, and your assessment of the
impact. Do not open a public issue for a security problem.

## Scope notes

- iPlus runs in industrial production environments; issues that are
  reachable remotely on the server components (`gip.iplus.service`,
  `gip.iplus.console`, `gip.core.webservices` incl. the MCP Server host
  on port 8750, the JSON/REST host on port 8730 (Basic-auth REST backend
  of the mobile MAUI client), and the SQL Tunnel host on port 8751) are
  treated with the highest priority.
- The framework and the MES application are two repositories:
  github.com/iplus-framework/iPlus (this one) and
  github.com/iplus-framework/iPlusMES. Report issues in the repository
  whose code is affected; if unsure, report here.

## Response

We aim to acknowledge reports within 7 days and will work with you on a
coordinated fix and disclosure.
