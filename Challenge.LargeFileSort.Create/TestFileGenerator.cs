using Challenge.LargeFileSort.Create.Generator;

namespace Challenge.LargeFileSort.Create
{
	class TestFileGenerator
	{
		static void Main(string[] args)
		{
			string outputFile;
			double targetSizeGB;

			if (args.Length < 2)
			{
				Console.WriteLine("No parameters were provided. Please input them manually.");
				Console.Write("Enter the output file path: ");
				outputFile = Console.ReadLine() ?? string.Empty;

				Console.Write("Enter the target file size in gigabytes (GB): ");
				string? sizeInput = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(outputFile) ||
					string.IsNullOrWhiteSpace(sizeInput) ||
					!double.TryParse(sizeInput, out targetSizeGB))
				{
					Console.WriteLine("Invalid input. The program will exit.");
					return;
				}
			}
			else
			{
				outputFile = args[0];
				if (!double.TryParse(args[1], out targetSizeGB))
				{
					Console.WriteLine("Invalid size input. The program will exit.");
					return;
				}
			}

			long targetSizeBytes = (long)(targetSizeGB * 1024 * 1024 * 1024);

			FileGeneratorHelper.GenerateTestFile(outputFile, targetSizeBytes);
		}
	}
}
