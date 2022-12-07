using System.Data.SqlClient;


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




	}
}
