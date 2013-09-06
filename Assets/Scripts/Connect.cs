using UnityEngine;
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net;

public static class Connect {
	private static MySqlConnection connection = new MySqlConnection();
	public static Dictionary<String,GameObject> hashes = new Dictionary<String,GameObject>();
	public static List<Action> pendingActions = new List<Action>();
	public static Dictionary<string, Action> responses = new Dictionary<string, Action>();
	public static List<string> UnitsToCreate = new List<string>();
	public static List<string> hashesToDelete = new List<string>();
	
	public static void ConnectToDB() {
		connection.ConnectionString =
			"server=localhost;" +
			"database=unity;" +
			"uid=unity;" +
			"password=unity;";
		try {
			connection.Open();
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	public static void LoadPlayers() {
		try {
			string sql = "SELECT * FROM players WHERE hash NOT IN (SELECT hash FROM todelete)";
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			MySqlDataReader result = cmd.ExecuteReader();
			
			while (result.Read())
				if (!hashes.ContainsKey(result[1].ToString()) && !UnitsToCreate.Contains(result[1].ToString()))
					UnitsToCreate.Add(result[1].ToString());
			result.Close();
			
			sql = "SELECT * FROM todelete";
			cmd = new MySqlCommand(sql, connection);
			result = cmd.ExecuteReader();
		
			while (result.Read()) {
				string hashToDelete = result[1].ToString();
				if (!hashesToDelete.Contains(hashToDelete))
					hashesToDelete.Add(hashToDelete);
			}
			result.Close();

			ExecSql("DELETE FROM todelete");
			ExecSql(String.Format("DELETE FROM players WHERE hash IN ('{0}')",
				String.Join("', '", hashesToDelete.ToArray())));
		}
		catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	public static void LoadActions() {
		try {
			// ExecSql("LOCK TABLES actions WRITE");

			string sql = "SELECT * FROM actions";
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			MySqlDataReader result = cmd.ExecuteReader();
			while (result.Read())
				pendingActions.Add(new Action(new string[] {
					(string) result[1].ToString(),
					(string) result[2].ToString(),
					(string) result[3].ToString()}));
			result.Close();
			
			ExecSql("DELETE FROM actions");
			// ExecSql("UNLOCK actions");
		}
		catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	public static void ResponseSpooler(string hash, string type, string data) {
		string rid = hash + type;
		if (responses.ContainsKey(rid))
			responses[rid] = new Action(new string[] {hash, type, data});
		else responses.Add(rid, new Action(new string[] {hash, type, data}));
	}
	
	public static void Respond () {
		if (responses.Count == 0) return;
		List<string> sqllist = new List<string>();
		foreach (KeyValuePair<string,Action> response in responses)
			sqllist.Add(String.Format("('{0}', '{1}', '{2}')",
				response.Value.hash, response.Value.type, response.Value.action));
		string sql = String.Format(
			"REPLACE INTO unityresponse (hash, type, data) VALUES {0}",
				String.Join(", ", sqllist.ToArray()));
		ExecSql(sql);
	}
	
	public static void ExecSql(string sql) {
		try {
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			MySqlDataReader result = cmd.ExecuteReader();
			result.Close();
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	public class Action {
		
		public string hash, type, action;
		
		public Action(string[] data) {
			hash   = data[0];
			type   = data[1];
			action = data[2];
		}
	}
}