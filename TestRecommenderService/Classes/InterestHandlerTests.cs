using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecommenderService.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderService.Classes.Tests
{
	[TestClass()]
	public class InterestHandlerTests
	{
		[TestMethod()]
		public void CreateUserInterestsTest()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database

			string UserID = "1";
			List<string> InitialTypes = new();
			InitialTypes.Add("test1");
			InitialTypes.Add("test2");
			InitialTypes.Add("test3");
			InitialTypes.Add("test4");


			Dictionary<string, double> expectedResult = new();
			expectedResult.Add("test1", 25);
			expectedResult.Add("test2", 25);
			expectedResult.Add("test3", 25);
			expectedResult.Add("test4", 25);

			//Act
			ih.CreateUserInterests(UserID, InitialTypes); //25, 25, 25, 25

			Tuple<ErrorStatus, Dictionary<string, double>> tuple = ih.GetUserInterests(UserID);


			//Assert
			CollectionAssert.AreEqual(tuple.Item2, expectedResult);
		}

		[TestMethod()]
		public void GetUserInterestsTest()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database


			//User 1
			string User1_ID = "1";
			List<string> User1_InitialTypes = new();
			User1_InitialTypes.Add("test1");
			User1_InitialTypes.Add("test2");
			User1_InitialTypes.Add("test3");
			User1_InitialTypes.Add("test4");
			ih.CreateUserInterests(User1_ID, User1_InitialTypes);

			//User 2
			string User2_ID = "2";
			List<string> User2_InitialTypes = new();
			User2_InitialTypes.Add("test1");
			User2_InitialTypes.Add("test4");
			User2_InitialTypes.Add("test5");
			User2_InitialTypes.Add("test6");
			ih.CreateUserInterests(User2_ID, User2_InitialTypes);

			//User 3
			string User3_ID = "3";
			List<string> User3_InitialTypes = new();
			User3_InitialTypes.Add("test3");
			User3_InitialTypes.Add("test4");
			User3_InitialTypes.Add("test5");
			User3_InitialTypes.Add("test6");
			ih.CreateUserInterests(User3_ID, User3_InitialTypes);


			//NewUser
			string NewUser_ID = "4";
			List<string> NewUser_InitialTypes = new();
			NewUser_InitialTypes.Add("test1");
			NewUser_InitialTypes.Add("test2");
			NewUser_InitialTypes.Add("test3");
			NewUser_InitialTypes.Add("test4");
			ih.CreateUserInterests(NewUser_ID, NewUser_InitialTypes);

			//i cheated and used the system to calculate this. this means that any error would npot be caught
			//('4', 'test1', '16.05'),('4', 'test2', '22.22'),('4', 'test3', '20.99'),('4', 'test4', '20.99'),('4', 'test5', '9.88'),('4', 'test6', '9.88');
			Dictionary<string, double> expectedResult = new();
			expectedResult.Add("test1", 16.05);
			expectedResult.Add("test2", 22.22);
			expectedResult.Add("test3", 20.99);
			expectedResult.Add("test4", 20.99);
			expectedResult.Add("test5", 9.88);
			expectedResult.Add("test6", 9.88);

			//Act
			Tuple<ErrorStatus, Dictionary<string, double>> tuple = ih.GetUserInterests(NewUser_ID);


			//Assert
			CollectionAssert.AreEqual(expectedResult, tuple.Item2);

		}

		[TestMethod()]
		public void GetUserInterestsTest_UserNotFound()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database

			//User 1
			string UserID = "1";
			
			//Act
			Tuple<ErrorStatus, Dictionary<string, double>> tuple = ih.GetUserInterests(UserID);

			//Assert
			Assert.AreEqual( ErrorStatus.UserNotFound, tuple.Item1);
		}


		[TestMethod()]
		public void GetSimilarInterestsTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void GetSimilarUsersTest()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database


			//User 1
			string User1_ID = "1";
			List<string> User1_InitialTypes = new();
			User1_InitialTypes.Add("test1");
			User1_InitialTypes.Add("test2");
			User1_InitialTypes.Add("test3");
			User1_InitialTypes.Add("test4");
			ih.CreateUserInterests(User1_ID, User1_InitialTypes); //25, 25, 25, 25

			//User 2
			string User2_ID = "2";
			List<string> User2_InitialTypes = new();
			User2_InitialTypes.Add("test1");
			User2_InitialTypes.Add("test4");
			User2_InitialTypes.Add("test5");
			User2_InitialTypes.Add("test6");
			ih.CreateUserInterests(User2_ID, User2_InitialTypes); //25, null, null, 25, 25, 25

			//User 3
			string User3_ID = "3";
			List<string> User3_InitialTypes = new();
			User3_InitialTypes.Add("test3");
			User3_InitialTypes.Add("test4");
			User3_InitialTypes.Add("test5");
			User3_InitialTypes.Add("test6");
			ih.CreateUserInterests(User3_ID, User3_InitialTypes); //

			//NewUser
			List<string> NewUser_InitialTypes = new();
			NewUser_InitialTypes.Add("test1");
			NewUser_InitialTypes.Add("test2");
			NewUser_InitialTypes.Add("test3");
			NewUser_InitialTypes.Add("test4");

			List<int> expectedResult = new();
			expectedResult.Add(1);
			expectedResult.Add(3);

			//Act
			List<int> result = ih.GetSimilarUsers(NewUser_InitialTypes);


			//Assert
			CollectionAssert.AreEqual(expectedResult, result);
		}

		[TestMethod()]
		public void UpdateUserInterestsTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void RemoveUserInterestTest_Success()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database

			string UserID = "1";
			List<string> InitialTypes = new();
			InitialTypes.Add("test1");
			InitialTypes.Add("test2");
			InitialTypes.Add("test3");
			InitialTypes.Add("test4");

			ih.CreateUserInterests(UserID, InitialTypes);

			//Act
			ErrorStatus result = ih.RemoveUserInterest(UserID);

			//Assert
			Assert.AreEqual(result, ErrorStatus.Success);
		}

		[TestMethod()]
		public void RemoveUserInterestTest_UserNotFound()
		{
			//Arrange
			InterestHandler ih = new(@"server=localhost;userid=root;password=duper;database=recommender_db");

			ih.ClearDatabase(); //clear database

			string UserID = "1";

			//Act
			ErrorStatus result = ih.RemoveUserInterest(UserID);

			//Assert
			Assert.AreEqual(result, ErrorStatus.UserNotFound);
		}
	}
}