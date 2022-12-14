using MySql.Data.MySqlClient;

namespace RecommenderService.Classes
{
	public static class ServiceTools
	{
		public static ErrorStatus CheckIfUserExist(string user_ID, string DB_table, MySqlConnection connection)
		{
			string SQLstatement = $"SELECT COUNT(*)" +
									$" FROM {DB_table}" +
									$" WHERE userid = {user_ID}";
			MySqlCommand command = new MySqlCommand(SQLstatement, connection);
			MySqlDataReader dataReader = command.ExecuteReader();


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

					return ErrorStatus.UserAlreadyExist; //Just one User
				}
				else if (idCount < 0)
				{
					dataReader.Close();
					command.Dispose();
					return ErrorStatus.UserCheckError; //Could not check
				}
				else if (idCount == 0)
				{
					dataReader.Close();
					command.Dispose();
					return ErrorStatus.UserNotFound; //No user exist
				}
				else
				{
					dataReader.Close();
					command.Dispose();
					return ErrorStatus.Unknown;
				}
				
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
