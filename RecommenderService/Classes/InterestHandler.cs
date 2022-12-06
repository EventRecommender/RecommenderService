﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderService.Classes
{
	public class InterestHandler
	{
		string connectionString { get; set; }
		SqlConnection connection { get; set; }
		

		public InterestHandler()
		{
			connectionString = @"";
			connection = new SqlConnection(connectionString);
		}

		public ErrorStatus CreateUserInterests(string user_ID, List<string> initial_types)
		{
			connection.Open();

			//Check if user exist:
			ErrorStatus userCheck = CheckIfUserExist(user_ID);

			if (userCheck != ErrorStatus.UserNotFound) //The user should not exist.
			{
				connection.Close();
				return userCheck;
			}

			//Calculate interests:
			Dictionary<string, float> interest = GetSimilarInterests(initial_types);

			StringBuilder sb = new StringBuilder("");
			foreach(KeyValuePair<string, float> kvp in interest)
			{
				sb.AppendLine($"INSERT INTO interest (userid, tag, value) values({user_ID}, {kvp.Key}, {kvp.Value})");
			}
			string SQLstatement = sb.ToString();

			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataAdapter adapter = new SqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			connection.Close();

			return ErrorStatus.Success;
		}

		public Dictionary<string, float> GetSimilarInterests(List<string> initial_types)
		{
			//Get similar users:
			List<int> similarUsersList = GetSimilarUsers(initial_types);


			// Get interests from similar users:
			StringBuilder sb = new StringBuilder("");
			foreach(int user in similarUsersList) 
			{
				sb.Append($"'{user}',");
			}
			sb.Remove(sb.Length - 1, 1); //removes trailing comma
			string similarUsers = sb.ToString();
			sb.Clear();

			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid IN ({similarUsers})";

			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			Dictionary<string, float> dict = new Dictionary<string, float>();

			while (dataReader.Read())
			{
				if (!dict.ContainsKey((string)dataReader[1])) //check if NOT exist
				{
					dict[(string)dataReader[1]] = (100 / initial_types.Count) / (similarUsersList.Count + 1);// +1 to account for the user
				}

				dict[(string)dataReader[1]] = (dict[(string)dataReader[1]] + ((float)dataReader[2] / (similarUsersList.Count + 1)));
			}

			dataReader.Close();
			command.Dispose();

			return dict;
		}

		public List<int> GetSimilarUsers(List<string> initial_types)
		{
			int val1 = 15;
			int val2 = 40; // used to stop outliers.

			StringBuilder sb = new StringBuilder("");
			foreach (string type in initial_types)
			{
				sb.Append($"'{type}',");
			}
			sb.Remove(sb.Length - 1, 1); //removes trailing comma
			string types = sb.ToString();
			sb.Clear();

			string SQLstatement = $"SELECT DISTINCT userid " +
									$"FROM (SELECT userid, COUNT(userid) as count" +
									$"			FROM interest" +
									$"			WHERE tag IN ('{types}')" +
									$"			AND tag IN" +
									$"				(SELECT * " +
									$"						FROM interest" +
									$"						WHERE value BETWEEN {val1} AND {val2})) " +
									$"WHERE count = 4"; // "count" is the number of types which are the same as those in initial_types.



			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();

			List<int> similarUsersList = new List<int>();

			while (dataReader.Read())
			{
				similarUsersList.Add((int)dataReader[0]);
			}
			dataReader.Close();
			command.Dispose();

			return similarUsersList;
		}

		public ErrorStatus UpdateUserInterests(string User_ID, List<string> activity_types, UpdateType Update_type)
		{
			connection.Open();

			//Determine value to update with
			int updateVal = 0;

			if (Update_type == UpdateType.Like)
			{
				updateVal = 5;
			}
			else if (Update_type == UpdateType.Dislike)
			{
				updateVal = -5;
			}

			//Check if user exist
			ErrorStatus userCheck = CheckIfUserExist(User_ID);

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				return userCheck;
			}

			//Get current values from database
			string SQLstatement =	$"SELECT * " +
									$"FROM interest " +
									$"WHERE userid == {User_ID}";

			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();


			//Save values in dictionary
			Dictionary<string, float> dict = new Dictionary<string, float>();
			while (dataReader.Read())
			{
				if (activity_types.Contains((string)dataReader[1]))
				{
					dict[(string)dataReader[1]] = (float)dataReader[2];
				}
			}
			dataReader.Close();
			command.Dispose();

			//update values in dictionary
			foreach (var tag in activity_types) 
			{
				dict[tag] = dict[tag] + updateVal;
			}

			//Find MIN and MAX values in dictionary
			KeyValuePair<string, float> min = dict.Aggregate((l, r) => l.Value < r.Value ? l : r);
			KeyValuePair<string, float> max = dict.Aggregate((l, r) => l.Value > r.Value ? l : r);

			//Normalize dictionary
			foreach (KeyValuePair<string, float> kvp in dict) 
			{
				dict[kvp.Key] = (kvp.Value - min.Value) / (max.Value - min.Value) * 100; // Zi = (xi - min(x)) / (max(x) - min(x)) * 100
			}

			//Update database with new values
			StringBuilder sb = new StringBuilder("");
			foreach (KeyValuePair<string, float> kvp in dict)
			{
				sb.AppendLine(	$"UPDATE interest " +
								$"SET {kvp.Key} = '{kvp.Value}'" +
								$"WHERE userid == {User_ID}");
			}
			SQLstatement = sb.ToString();

			command = new SqlCommand(SQLstatement, connection);
			SqlDataAdapter adapter = new SqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			//Finished
			connection.Close();
			return ErrorStatus.Success;
		}

		public ErrorStatus CheckIfUserExist(string user_ID) 
		{
			string SQLstatement = $"SELECT COUNT(*)" +
										$" FROM interest" +
										$" WHERE userid = {user_ID}";
			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();


			string idCountString = "-1"; //used to check for error
			while (dataReader.Read())
			{
				idCountString = dataReader.GetString(0);
			}

			
			try
			{
				var idCount = int.Parse(idCountString);

				if (idCount > 0)
				{
					dataReader.Close();
					command.Dispose();
					if (idCount > 1)
					{
						return ErrorStatus.DublicateUser; //More than one
					}
					return ErrorStatus.UserAlreadyExist; //Just one User
				}
				else if (idCount < 0)
				{
					return ErrorStatus.UserCheckError; //Could not check
				}
				dataReader.Close();
				command.Dispose();
				return ErrorStatus.UserNotFound; //No user exist
			}
			catch (FormatException)
			{
				dataReader.Close();
				command.Dispose();
				connection.Close();
				return ErrorStatus.UserCheckError;	//Could not check
			}
		}

	}
}
