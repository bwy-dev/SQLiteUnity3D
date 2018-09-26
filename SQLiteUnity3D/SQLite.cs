using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using UnityEngine;

namespace SQLiteUnity3D
{
	public class SQLite
	{
		public static string dbPath;

		public static void SetPath(string path)
		{
			dbPath = "URI=file:" + Application.persistentDataPath + "/" + path + ".db";
			CreateDatabase(path);
			Debug.Log(dbPath);
		}

		private static void CreateDatabase(string dbName = "database")
		{
			string pathString = Application.persistentDataPath + "/" + dbName + ".db";

			if(!System.IO.File.Exists(pathString))
			{
				System.IO.FileStream fs = System.IO.File.Create(pathString);			
			}
		}

		/// <summary>
		/// Creates a new SQLite table with "title" as its name, "pkName" as primary key name, and "columns" as column name and types.
		/// </summary>
		public static void CreateTable(string title, string pkName, SortedDictionary<string, Type> columns)
		{
			string sqlInput = "CREATE TABLE IF NOT EXISTS '" + title + "' ( '" + pkName + "' INTEGER PRIMARY KEY";

			foreach (var pair in columns)
			{
				string dataType = null;
				if (pair.Value == typeof(int))
				{
					dataType = "INTEGER NOT NULL";
				}
				else if (pair.Value == typeof(string) || pair.Value == typeof(char))
				{
					dataType = "TEXT NOT NULL";
				}
				else if (pair.Value == typeof(float) || pair.Value == typeof(double))
				{
					dataType = "REAL NOT NULL";
				}
				else
				{
					throw new ArgumentException("dataType cannot be null, dataType can only be of Type int, string, char, float or double");
				}

				Regex r = new Regex("^[a-zA-Z0-9]*$");
				if (r.IsMatch(pair.Key))
				{
					sqlInput += ", '" + pair.Key + "' " + dataType;
				}
				else
				{
					throw new ArgumentException("data must contain only alphanumeric characters");
				}
			}
			sqlInput += ");";

			using (var conn = new SqliteConnection(dbPath))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = sqlInput;

					var result = cmd.ExecuteNonQuery();
					Debug.Log("create schema: " + result);
				}
			}
		}

		public static void AddRow(string tableName, object insertedValues)
		{
			
			Type type = insertedValues.GetType();
			PropertyInfo[] properties = type.GetProperties();
			List<string> columnNames = GetColumnNames(tableName);
			string valTableName = CheckString(tableName);
			string sqlInput = "INSERT INTO '" + valTableName + "VALUES ( " ; 

			foreach (PropertyInfo property in properties)
			{
				var value = property.GetValue(property);

				if (value is string)
				{
					value = CheckString(Convert.ToString(value));
				}

				sqlInput += value + ", ";
				
			}

			using (var conn = new SqliteConnection(dbPath))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = sqlInput;
				}
			}
		}

		private static List<string> GetColumnNames(string tableName)
		{
			using (var con = new SqliteConnection(dbPath))
			{
				using (var cmd = new SqliteCommand("PRAGMA table_info(" + tableName + ");"))
				{
					var table = new DataTable();
					cmd.Connection = con;
					cmd.Connection.Open();

					SqliteDataAdapter adp = null;
					try
					{
						adp = new SqliteDataAdapter(cmd);
						adp.Fill(table);
						con.Close();
						var res = new List<string>();
						for (int i = 0; i < table.Rows.Count; i++)
							res.Add(table.Rows[i]["name"].ToString());
						return res;
					}
					catch (Exception ex) { }
				}
			}
			return new List<string>();
		}

		private static string CheckString(string s)
		{
			Regex r = new Regex("^[a-zA-Z0-9]*$");
			if (!r.IsMatch(s))
			{
				throw new ArgumentException("data must contain only alphanumeric characters");	
			}
			else
			{
				return s;
			}
		}


	}
}
