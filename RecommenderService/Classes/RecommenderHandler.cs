using Microsoft.AspNetCore.Http;
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
	public class RecommenderHandler
	{
		string connectionString { get; set; }
		SqlConnection connection { get; set; }


		public RecommenderHandler()
		{
			connectionString = @"";
			connection = new SqlConnection(connectionString);
		}

		public Tuple<ErrorStatus, Dictionary<string, int>> GetRecommendation(string User_ID)
		{
			connection.Open();


			//Check if user exist.
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "recommendation", connection);
			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return new Tuple<ErrorStatus, Dictionary<string, int>>(userCheck, new Dictionary<string, int>());
			}

			//Get recommendation from database
			string SQLstatement =	$"Select * " +
									$"FROM recommendation " +
									$"WHERE userid == {User_ID}";

			SqlCommand command = new SqlCommand(SQLstatement, connection);
			SqlDataReader dataReader = command.ExecuteReader();


			//populate dict with the data
			Dictionary<string, int> dict = new Dictionary<string, int>();

			while (dataReader.Read())
			{
				dict[(string)dataReader[1]] = (int)dataReader[2];
			}



			connection.Close();
			return new Tuple<ErrorStatus, Dictionary<string, int>>(ErrorStatus.Success, dict);
		}

		public ErrorStatus CalculateRecommendation(string User_ID, int amountOfRecommendations = 10)
		{
			connection.Open();

			//Check if user exist.
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "recommendation", connection);
			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return userCheck;
			}

			//get user interests
			InterestHandler ih = new InterestHandler();

			var temp = ih.GetUserInterests(User_ID);
			if (temp.Item1 != ErrorStatus.Success)
			{
				connection.Close();
				return temp.Item1;
			}

			Dictionary<string, float> dictInterests = temp.Item2;


			//sorting the dictionary, which changes the dictionary which is then converted back to a dictionary.
			var sortedDict = (from entry in dictInterests orderby entry.Value ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

			Dictionary<string, int> amountDict = new Dictionary<string, int>();

			//Calculation of how many activities of each type.
			//Current calculation type: Percentage
			float amount = 0;
			foreach (var kvp in sortedDict)
			{
				amount = (kvp.Value / 100) * amountOfRecommendations; 
				amountDict.Add(kvp.Key, (int)Math.Round(amount));
			}

			//Insert into database
			StringBuilder sb = new StringBuilder("");
			foreach (KeyValuePair<string, int> kvp in amountDict)
			{
				sb.AppendLine($"INSERT INTO recommendation (userid, tag, value) values({User_ID}, {kvp.Key}, {kvp.Value})");
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


	}
}
