using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Mono.Data.Sqlite;
using UnityEngine;

namespace SQLiteUnity3D
{
	public class SQLite
	{
		public static string dbPath;

		public static void SetPath(string path)
		{
			dbPath = "URI=file:" + Application.persistentDataPath + "/" + path;
			Debug.Log(dbPath);
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

				sqlInput += ", '" + pair.Key + "' " + dataType;
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

		public static void AddToTable(string tableName, object insertedValues)
		{
			Type type = insertedValues.GetType();
			PropertyInfo[] properties = type.GetProperties();
			List<string> columnNames = GetColumnNames(tableName);
			string sqlInput = null; 

			foreach (PropertyInfo property in properties)
			{
				var value = property.GetValue(property);
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
	}
}
