using RecommenderService.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.



app.MapGet("/CalculateRecommendation", (string User_ID) =>
{
	//Get should not change anything on the database... but... we do it anyway

	//TODO: Do stuff
	return something;
});

app.MapGet("/GetRecommendation", (string User_ID) =>
{
	//TODO: Do stuff
	return something;
});

app.MapPost("/RemoveUserInterests", (string User_ID) =>
{
	//TODO: Do stuff
	return something;
});

app.MapPost("/CreateUserInterests", (string user_ID, List<string> initial_types) =>
{
	try
	{
		InterestHandler handler = new InterestHandler();
		ErrorStatus result = handler.CreateUserInterests(user_ID, initial_types);

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
	catch (Exception)
	{
		return Results.Problem("Unhandled exception occured");
	}
});

app.MapPost("/UpdateUserInterests", (string User_ID, List<string> activity_types, UpdateType Update_type) =>
{
	//TODO: Do stuff
	return something;
});


app.Run();
