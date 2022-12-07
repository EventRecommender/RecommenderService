﻿using System.Data.SqlClient;

namespace RecommenderService.Classes
{
	public static class ServiceTools
	{
		public static ErrorStatus CheckIfUserExist(string user_ID, string DB_table, SqlConnection connection)
		{
			string SQLstatement = $"SELECT COUNT(*)" +
									$" FROM {DB_table}" +
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
				return ErrorStatus.UserCheckError;  //Could not check
			}
		}
	}
}
