using RecommenderService.Classes;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

//test
app.MapPost("/", (string tests) =>
{
	string cs = @"server=mysql_recommender;userid=root;password=duper;database=recommender_db";

	using var con = new MySqlConnection(cs);
	con.Open();
	Console.WriteLine(tests[0]);
	return ($"MySQL version : {con.ServerVersion}");
});

app.MapPost("/CalculateRecommendation", (string userid) =>
{
	try
	{
		RecommenderHandler handler = new();

		ErrorStatus result = handler.CalculateRecommendation(userid);

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
	catch (Exception ex)
	{
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});

app.MapGet("/GetRecommendation", (string userid) =>
{
	try
	{
		RecommenderHandler handler = new();

		var result = handler.GetRecommendation(userid);

		if (result.Item1 == ErrorStatus.Success)
		{
			string json = JsonSerializer.Serialize<Dictionary<string, int>>(result.Item2);

			return Results.Ok(json);
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
	catch (Exception ex)
	{
		return Results.Problem("Unhandled exception occured " + "     exType: " + ex.GetType() + "     Message: " + ex.Message + "     StackTrace: " + ex.StackTrace);
	}
});

app.MapPost("/RemoveUserInterests", (string userid) =>
{
	try
	{
		InterestHandler handler = new InterestHandler();

		ErrorStatus result = handler.RemoveUserInterest(userid);

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
		List<String> types = JsonSerializer.Deserialize<List<String>>(initial_types);
		InterestHandler handler = new InterestHandler();
		ErrorStatus result = handler.CreateUserInterests(userid, types);

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

app.MapPost("/UpdateUserInterests", (string userid, string activity_types, int update_type) =>
{
	try
	{
		List<string> types = JsonSerializer.Deserialize<List<string>>(activity_types);

		InterestHandler handler = new InterestHandler();

		ErrorStatus result = handler.UpdateUserInterests(userid, types, update_type);

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
