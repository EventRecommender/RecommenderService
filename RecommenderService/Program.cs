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
	//TODO: Do stuff
	return something;
});

app.MapPost("/UpdateUserInterests", (string User_ID, List<string> activity_types, UpdateType Update_type) =>
{
	//TODO: Do stuff
	return something;
});


app.Run();
