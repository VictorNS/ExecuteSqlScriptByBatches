using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using PowerArgs;

namespace ExecuteSqlScriptByBatches
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.White;
			MyArgs pars;
			try
			{
				pars = Args.Parse<MyArgs>(args);
			}
			catch (ArgException ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<MyArgs>());
				return;
			}

			if (string.IsNullOrEmpty(pars.ConnectionString))
			{
				try
				{
					pars.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
				}
				catch (ArgException ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}

			try
			{
				using (SqlConnection connection = new SqlConnection(pars.ConnectionString))
				{
					connection.Open();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return;
			}

			try
			{
				Int64 counter = 0;
				bool interrupt = false;
				var batch = new StringBuilder();
				int batchSize = 0;
				string line;
				using (StreamReader sr = File.OpenText(pars.File))
				{
					while (!interrupt)
					{
						counter++;
						if ((line = sr.ReadLine()) == null)
						{
							line = "";
							interrupt = true;
							batchSize = int.MaxValue - 10;
						}

						#region gathering batch
						if (line != "GO")
							batch.AppendLine(line);

						if (pars.FindGo)
						{
							if (line == "GO")
								batchSize++;
						}
						else
						{
							batchSize++;
						}
						#endregion gathering batch

						if (batchSize >= pars.BatchSize)
						{
							#region execute
							if (pars.ShowQuery)
								Console.WriteLine(batch.ToString());
							Console.WriteLine($"{counter} lines read... ");

							try
							{
								using (SqlConnection connection = new SqlConnection(pars.ConnectionString))
								{
									SqlCommand command = new SqlCommand(batch.ToString(), connection);
									command.Connection.Open();
									command.ExecuteNonQuery();
								}
							}
							catch (Exception sqlEx)
							{
								Console.WriteLine();
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine(sqlEx);
								Console.ForegroundColor = ConsoleColor.White;
							}
							batch.Clear();
							batchSize = 0;
							#endregion execute
						}
					}
				}
			}
			catch (ArgException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
	public class MyArgs
	{
		[ArgRequired()]
		public string File { get; set; }

		[ArgDefaultValue(true)]
		public bool FindGo { get; set; }

		[ArgRange(0, 100), ArgDefaultValue(0)]
		public int BatchSize { get; set; }

		public string ConnectionString { get; set; }

		[ArgDefaultValue(false)]
		public bool ShowQuery { get; set; }
	}
}
