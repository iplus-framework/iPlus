using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using combit.ListLabel17;

namespace gip.core.reporthandler
{
	internal class UsedIdentifierHelper
	{
		private ReadOnlyCollection<string> _identifiers;
		private Dictionary<string, ReadOnlyCollection<string>> _identifiersPerTable;
		private Dictionary<string, bool> _hasIdentifiersStartingWith = new Dictionary<string, bool>();
		public UsedIdentifierHelper(ReadOnlyCollection<string> identifiers)
		{
			this._identifiers = identifiers;
			this._identifiersPerTable = new Dictionary<string, ReadOnlyCollection<string>>();
		}
		public bool HasIdentifiersStartingWith(string value, bool caseSensitive)
		{
			if (!caseSensitive)
			{
				value = value.ToLowerInvariant();
			}
			bool result;
			if (this._hasIdentifiersStartingWith.TryGetValue(value, out result))
			{
				return result;
			}
			foreach (string current in this._identifiers)
			{
				if (!caseSensitive)
				{
					string text = current.ToLowerInvariant();
					if (text.StartsWith(value))
					{
						this._hasIdentifiersStartingWith.Add(value, true);
						bool result2 = true;
						return result2;
					}
				}
				else
				{
					if (current.StartsWith(value))
					{
						this._hasIdentifiersStartingWith.Add(value, true);
						bool result2 = true;
						return result2;
					}
				}
			}
			this._hasIdentifiersStartingWith.Add(value, false);
			return false;
		}
		public ReadOnlyCollection<string> GetIdentifiersForTable(string tableName)
		{
			ReadOnlyCollection<string> readOnlyCollection;
			if (this._identifiersPerTable.TryGetValue(tableName, out readOnlyCollection))
			{
				return readOnlyCollection;
			}
			List<string> list = new List<string>();
			foreach (string current in this._identifiers)
			{
				if (current.Contains(tableName + "."))
				{
					int num = current.LastIndexOf('@');
					int num2 = current.LastIndexOf(':');
					if (num != -1 || num2 != -1)
					{
						if (num != -1)
						{
							string text = current.Substring(num + 1);
							string a = text.Substring(0, text.LastIndexOf('.'));
							if (!(a != tableName))
							{
								string item = current.Substring(num2 + 1);
								if (!list.Contains(item))
								{
									list.Add(item);
								}
							}
						}
						else
						{
							if (num2 != -1)
							{
								string text2 = current.Substring(num2 + 1);
								int num3 = text2.LastIndexOf('.');
								string a2 = text2.Substring(0, num3);
								if (!(a2 != tableName))
								{
									string item2 = text2.Substring(num3 + 1);
									if (!list.Contains(item2))
									{
										list.Add(item2);
									}
								}
							}
						}
					}
					else
					{
						int num4 = tableName.Length + 1;
						string a3 = current.Substring(0, num4);
						if (a3 == tableName)
						{
							string item3 = current.Substring(num4 + 1);
							if (!list.Contains(item3))
							{
								list.Add(item3);
							}
						}
					}
				}
			}
			readOnlyCollection = list.AsReadOnly();
			this._identifiersPerTable[tableName] = readOnlyCollection;
			return readOnlyCollection;
		}
	}
}
