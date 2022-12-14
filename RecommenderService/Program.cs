using RecommenderService.Classes;
using RecommenderService.Exceptions;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


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
			return Results.Problem("User already exist");
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (ConnectionException e)
	{
		MySqlConnection.ClearPool(e.con);
		return Results.Problem("Unhandled Exception occured with MySql");
	}
	catch (Exception e)
	{
		return Results.Problem("Unhandled exception occured");
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
			return Results.BadRequest("User not found");
		}
		else if (result.Item1 == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty");
		}
		else if (result.Item1 == ErrorStatus.DataOutdated)
		{
			return Results.BadRequest("Recommendation out of date");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (ConnectionException e)
	{
		MySqlConnection.ClearPool(e.con);
		return Results.Problem("Unhandled Exception occured with MySql");
	}
	catch (Exception ex)
	{
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
			return Results.Problem("User not found");
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (Exception ex)
	{
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
			return Results.BadRequest("User already exist");
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (Exception ex)
	{
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
			return Results.BadRequest("User does not exist");
		}
		else if (result == ErrorStatus.QueryStringEmpty)
		{
			return Results.Problem("The query string was empty");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (Exception ex)
	{
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});


app.Run();
