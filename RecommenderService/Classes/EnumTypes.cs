﻿namespace RecommenderService.Classes
{
	public enum UpdateType
	{
		Like,
		Dislike
	}

	public enum ErrorStatus
	{
		Success,
		UserNotFound,
		DublicateUser
	}
}