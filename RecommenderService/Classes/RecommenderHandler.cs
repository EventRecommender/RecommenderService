using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RecommenderService.Classes
{
	public class RecommenderHandler
	{
		string connectionString { get; set; }
		MySqlConnection connection { get; set; }


		public RecommenderHandler()
		{
			connectionString = @"server=mysql_recommender;userid=root;password=duper;database=recommender_db";
			connection = new MySqlConnection(connectionString);
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
									$"WHERE userid = {User_ID}";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();


			//populate dict with the data
			Dictionary<string, int> dict = new Dictionary<string, int>();

			DateTime currentDate = DateTime.Now;
			DateTime date;
			while (dataReader.Read())
			{
				date = (DateTime)dataReader[3];
				if ((date.Year < currentDate.Year) || (date.DayOfYear < currentDate.DayOfYear)) //check if data is out of date
				{
					connection.Close();
					return new Tuple<ErrorStatus, Dictionary<string, int>>(ErrorStatus.DataOutdated, new Dictionary<string, int>());
				} 
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
			connection.Close();
			ErrorStatus deleted = ErrorStatus.None;
			if (userCheck != ErrorStatus.UserNotFound)
			{
				deleted = RemoveRecommendation(User_ID);

				if (deleted != ErrorStatus.Success)
				{
					
					return deleted;
				}
			}



			connection.Open();

			//get user interests
			InterestHandler ih = new InterestHandler();

			var temp = ih.GetUserInterests(User_ID);
			if (temp.Item1 != ErrorStatus.Success)
			{
				connection.Close();
				return temp.Item1;
			}

			Dictionary<string, double> dictInterests = temp.Item2;


			//sorting the dictionary, which changes the dictionary which is then converted back to a dictionary.
			//This is deone because the recommendations is shown to the user in order form most important to least.
			var sortedDict = (from entry in dictInterests orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

			Dictionary<string, int> amountDict = new Dictionary<string, int>();

			//Calculation of how many activities of each type.
			//Current calculation type: Percentage
			double amount = 0;
			foreach (var kvp in sortedDict)
			{
				amount = (kvp.Value / 100) * amountOfRecommendations; 
				amountDict.Add(kvp.Key, (int)Math.Round(amount));
			}

			//Create insert SQL
			StringBuilder sb = new StringBuilder($"INSERT INTO recommendation (userid, tag, amount, creationdate) VALUES ");

			List<string> rows = new List<string>();
			foreach (KeyValuePair<string, int> kvp in amountDict)
			{
				rows.Add(string.Format("('{0}','{1}', '{2}', '{3}')", MySqlHelper.EscapeString(User_ID), MySqlHelper.EscapeString(kvp.Key), MySqlHelper.EscapeString((kvp.Value).ToString()), (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
			}
			sb.Append(string.Join(",", rows));
			sb.Append(";");

			string SQLstatement = sb.ToString();

			if (string.IsNullOrEmpty(SQLstatement))
			{
				connection.Close();
				return ErrorStatus.QueryStringEmpty;
			}

			//Insert into DB
			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataAdapter adapter = new MySqlDataAdapter();

			command.CommandType = CommandType.Text;
			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();

			connection.Close();
			return ErrorStatus.Success;
		}

		public ErrorStatus RemoveRecommendation(string User_ID)
		{
			connection.Open();
			//Check if user exist
			ErrorStatus userCheck = ServiceTools.CheckIfUserExist(User_ID, "recommendation", connection);

			if (userCheck != ErrorStatus.UserAlreadyExist)
			{
				connection.Close();
				return userCheck;
			}

			//Delete records related to user.
			string SQLstatement =	$"DELETE " +
									$"FROM recommendation " +
									$"WHERE userid = {User_ID}";

			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataAdapter adapter = new MySqlDataAdapter();

			adapter.InsertCommand = command;
			adapter.InsertCommand.ExecuteNonQuery();

			command.Dispose();
			adapter.Dispose();


			connection.Close();
			return ErrorStatus.Success;
		}


	}
}
