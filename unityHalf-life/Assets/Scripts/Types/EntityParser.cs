using System;
using System.Collections.Generic;



	public class EntityParser
	{
		private string entities;
		private int length;
		private int position;
		
		public EntityParser(string entities)
		{
			this.entities = entities;
			length = entities.Length;
			position = 0;
		}
		
		private bool IsWhiteSpace(char c)
		{
			return ((c == ' ') || (c == '\t') || (c == '\r') || (c == '\n'));
		}
		
		private void ReadWhiteSpaces()
		{
			while (IsWhiteSpace(entities[position])) {
				position++;        
			}
		}
		
		private void Expect(char c)
		{
			if (position >= length) {
				throw new Exception(string.Format("Expected {0} but reached end", c));
			} else if (entities[position] == c) {
				position++;
			} else {
				throw new Exception(string.Format("Expected {0} at position {1}", c, position));        
			}
		}
		
		private void ReadUntil(char c)
		{
			while (entities[position] != c) {
				if (position >= length) {
					throw new Exception(string.Format("Expected {0} but reached end", c));
				}
				position++;
			}
			position++;
		}
		
		private string ReadValue()
		{
			ReadWhiteSpaces();
			Expect('\"');
			int start = position;
			ReadUntil('\"');
			int length = position - start - 1;
			
			string ret = entities.Substring(start, length);
			return ret;
		}
		
		public Dictionary<string, string> ReadEntity()
		{
			ReadWhiteSpaces();
			
			if (entities[position] != '{') {
				return null;        
			}
			
			Dictionary<string, string> dict = new Dictionary<string, string>();
			
			Expect('{');
			
			dict.Add(ReadValue(), ReadValue());
			
			ReadWhiteSpaces();
			
			while (entities[position] != '}') {
				string key = ReadValue();
				string val = ReadValue();
				if (dict.ContainsKey(key)) {
					if (dict[key] != val) throw new Exception("Missdefined class");
				} else {
					dict.Add(key, val);
				}
				ReadWhiteSpaces();
			}
			position++;
			
			return dict;
		}
		
		public Dictionary<string, List<Dictionary<string, string>>> ReadEntities()
		{
			Dictionary<string, List<Dictionary<string, string>>> ret = new Dictionary<string, List<Dictionary<string, string>>>();
			
			foreach (var entity in Entities) {
				if (!ret.ContainsKey(entity["classname"])) {
					ret[entity["classname"]] = new List<Dictionary<string, string>>();
				}
				ret[entity["classname"]].Add(entity);
			}
			return ret;
		}
		
		public IEnumerable<Dictionary<string, string>> Entities {
			get {
				Dictionary<string, string> entity = null;
				while ((entity = ReadEntity()) != null) {
					yield return entity;
				}
			}
		}
	}
