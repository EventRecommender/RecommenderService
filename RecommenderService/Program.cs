using RecommenderService.Classes;
using RecommenderService.Exceptions;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);


InterestHandler interestHandler = new();
RecommenderHandler recommenderHandler = new();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("/CalculateRecommendation", (string userid) =>
{
	try
	{
		ErrorStatus result = recommenderHandler.CalculateRecommendation(userid.ToLower());

		if (result == ErrorStatus.Success)
		{
			return Results.Ok();
		}
		else if (result == ErrorStatus.UserAlreadyExist)
		{
			Console.WriteLine("User already exist - UserID: " + userid);
			return Results.BadRequest("User already exist - UserID: " + userid);
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty - UserID: " + userid);
		}
		else
		{
			//Unhandled Result
			Console.WriteLine("Unhandled result - UserID: " + userid + "   result: " + result);
			return Results.BadRequest("Unhandled result - UserID: " + userid + "  result: " + result);
		}
	}
	catch (ConnectionException e)
	{
		MySqlConnection.ClearPool(e.con);
		Console.WriteLine("Exception: " + e.Message);
		return Results.Problem("Unhandled Exception occured with MySql - UserID: " + userid);
	}
	catch (Exception e)
	{
		Console.WriteLine("Exception: " + e.Message);
		return Results.Problem("Unhandled exception occured - UserID: " + userid);
	}
});

app.MapGet("/GetRecommendation", (string userid) =>
{
	try
	{
		var result = recommenderHandler.GetRecommendation(userid.ToLower());

		if (result.Item1 == ErrorStatus.Success)
		{
			return Results.Json(result.Item2);
		}
		else if (result.Item1 == ErrorStatus.UserNotFound)
		{
			Console.WriteLine("User not found - Userid: " + userid);
			return Results.BadRequest("User not found - Userid: " + userid);
		}
		else if (result.Item1 == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty - UserID: " + userid);
		}
		else if (result.Item1 == ErrorStatus.DataOutdated)
		{
			Console.WriteLine("Recommendation out of date for User: " + userid);
			return Results.BadRequest("Recommendation out of date for User: " + userid);
		}
		else if (result.Item1 == ErrorStatus.resultEmpty)
		{
			return Results.Problem("Error occurred while retrieving recommendation from User: " + userid);
		}
		else
		{
			//Unhandled Result
			Console.WriteLine("Unhandled result - UserID: " + userid + "   result: " + result);
			return Results.BadRequest("Unhandled result - UserID: " + userid + "   result: " + result);
		}
	}
	catch (ConnectionException e)
	{
		MySqlConnection.ClearPool(e.con);
		Console.WriteLine("Exception: " + e.Message);
		return Results.Problem("Unhandled Exception occured with MySql - UserID: " + userid);
	}
	catch (Exception ex)
	{
		Console.WriteLine("Exception: " + ex.Message);
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});

app.MapPost("/RemoveUserInterests", (string userid) =>
{
	try
	{
		ErrorStatus result = interestHandler.RemoveUserInterest(userid.ToLower());

		if (result == ErrorStatus.Success)
		{
			return Results.Ok();
		}
		else if (result == ErrorStatus.UserNotFound)
		{
			return Results.Problem("User not found - Userid: " + userid);
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty - Userid: " + userid);
		}
		else
		{
			//Unhandled Result
			Console.WriteLine("Unhandled result - UserID: " + userid + "   result: " + result);
			return Results.BadRequest("Unhandled result - Userid: " + userid + "   result: " + result);
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine("Exception: " + ex.Message);
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});

app.MapPost("/CreateUserInterests", (string userid, string initial_types) =>
{
	try
	{
		List<String> types = JsonSerializer.Deserialize<List<String>>(initial_types.ToLower());
		ErrorStatus result = interestHandler.CreateUserInterests(userid.ToLower(), types);

		if (result == ErrorStatus.Success)
		{
			return Results.Ok();
		}
		else if (result == ErrorStatus.UserAlreadyExist)
		{
			Console.WriteLine("User already exist - UserID: " + userid);
			return Results.BadRequest("User already exist - UserID: " + userid);
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty - UserID: " + userid);
		}
		else
		{
			Console.WriteLine("Unhandled result - UserID: " + userid + "   result: " + result);
			//Unhandled Result
			return Results.BadRequest("Unhandled result - UserID: " + userid + "   result: " + result);
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine("Exception: " + ex.Message);
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});

app.MapPost("/UpdateUserInterests", (string userid, string activity_types, UpdateType update_type) =>
{
	try
	{
		List<string> types = JsonSerializer.Deserialize<List<string>>(activity_types.ToLower());

		ErrorStatus result = interestHandler.UpdateUserInterests(userid.ToLower(), types, update_type);

		if (result == ErrorStatus.Success)
		{
			return Results.Ok();
		}
		else if (result == ErrorStatus.UserNotFound)
		{
			Console.WriteLine("User does not exist - Userid: " + userid);
			return Results.BadRequest("User does not exist - Userid: " + userid);
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty - Userid: " + userid);
		}
		else
		{
			//Unhandled Result
			Console.WriteLine("Unhandled result - UserID: " + userid + "   result: " + result);
			return Results.BadRequest("Unhandled result - Userid: " + userid);
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine("Exception: " + ex.Message);
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});


app.Run();
