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
			InterestHandler ih = new();

			//User 1
			string UserID = "1";
			List<string> User1_InitialTypes = new();
			User1_InitialTypes.Add("test1");
			User1_InitialTypes.Add("test2");
			User1_InitialTypes.Add("test3");
			User1_InitialTypes.Add("test4");

			//Act
			ih.CreateUserInterests(UserID, User1_InitialTypes); //25, 25, 25, 25

			Tuple<ErrorStatus, Dictionary<string, double>> tuple = ih.GetUserInterests(UserID);

			//Assert
			Assert.Fail();
		}

		[TestMethod()]
		public void GetUserInterestsTest()
		{
			//Arrange
			InterestHandler ih = new();

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
			ih.CreateUserInterests(User2_ID, User2_InitialTypes); //25, 12.5, 12.5, 25, 12.5, 12.5

			//User 3
			string User3_ID = "3";
			List<string> User3_InitialTypes = new();
			User3_InitialTypes.Add("test3");
			User3_InitialTypes.Add("test4");
			User3_InitialTypes.Add("test5");
			User3_InitialTypes.Add("test6");
			ih.CreateUserInterests(User3_ID, User3_InitialTypes); //12.5, 6.25, 18,75, 25, 18.75, 18.75




			//NewUser
			string NewUser_ID = "4";
			List<string> NewUser_InitialTypes = new();
			NewUser_InitialTypes.Add("test1");
			NewUser_InitialTypes.Add("test2");
			NewUser_InitialTypes.Add("test3");
			NewUser_InitialTypes.Add("test4");
			ih.CreateUserInterests(NewUser_ID, NewUser_InitialTypes); //18.75, 15.625, 21.875, 25, 9.375, 9.375

			Dictionary<string, double> expectedResult = new();
			expectedResult.Add("test1", 18.75);
			expectedResult.Add("test2", 15.625);
			expectedResult.Add("test3", 21.875);
			expectedResult.Add("test4", 25);
			expectedResult.Add("test5", 9.375);
			expectedResult.Add("test6", 9.375);

			//Act
			Tuple<ErrorStatus, Dictionary<string, double>> tuple = ih.GetUserInterests(NewUser_ID);

			//Assert
			Assert.Equals(expectedResult, tuple.Item2);
		}

		[TestMethod()]
		public void GetSimilarInterestsTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void GetSimilarUsersTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void UpdateUserInterestsTest()
		{
			Assert.Fail();
		}

		[TestMethod()]
		public void RemoveUserInterestTest()
		{
			Assert.Fail();
		}
	}
}