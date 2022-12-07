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

app.MapGet("/CalculateRecommendation", (string User_ID) =>
{
	//Get should not change anything on the database... but... we do it anyway

	//TODO: Do stuff
	return Results.Problem("not implemented");
});

app.MapGet("/GetRecommendation", (string User_ID) =>
{
	//TODO: Do stuff
	return Results.Problem("not implemented");
});

app.MapPost("/RemoveUserInterests", (string User_ID) =>
{
	//TODO: Do stuff
	return Results.Problem("not implemented");
});

app.MapPost("/CreateUserInterests", (string user_ID, string initial_types) =>
{
	try
	{
		List<String> types = JsonSerializer.Deserialize<List<String>>(initial_types);
		InterestHandler handler = new InterestHandler();
		ErrorStatus result = handler.CreateUserInterests(user_ID, types);

		if (result == ErrorStatus.Success)
		{
			return Results.Ok();
		}
		else if (result == ErrorStatus.UserAlreadyExist)
		{
			return Results.BadRequest("User already exist");
		}
		else if (result == ErrorStatus.DublicateUser)
		{
			return Results.BadRequest("Dublicate Users already exists");
		}
		else
		{
			//Unhandled Result
			return Results.BadRequest("Unhandled result");
		}
	}
	catch (Exception ex)
	{
		return Results.Problem("Unhandled exception occured" + ex.GetType() + ex.Message);
	}
});

app.MapPost("/UpdateUserInterests", (string User_ID, List<string> activity_types, UpdateType Update_type) =>
{
	//TODO: Do stuff
	return Results.Problem("not implemented");
});


app.Run();
