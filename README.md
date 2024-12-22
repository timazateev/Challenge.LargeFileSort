# README: Using FileGenerator and FileSorter

This document explains how to use the **FileGenerator** and **FileSorter** tools for generating and sorting large files efficiently. Examples of usage via console commands with and without parameters are also provided.

---

## Getting Started

### Cloning the Repository
To use the tools, first clone the repository to your local machine:
```bash
git clone <repository-url>
cd Challenge.LargeFileSort
```

### Building the Projects
Each tool is located in its respective project folder and needs to be built before execution. Use your preferred IDE (e.g., Visual Studio) or .NET CLI to build the projects:

1. **FileGenerator**:
   Navigate to the `Challenge.LargeFileSort.Create` folder and build the project:
   ```bash
   cd Challenge.LargeFileSort.Create
   dotnet build
   ```

2. **FileSorter**:
   Navigate to the `Challenge.LargeFileSort` folder and build the project:
   ```bash
   cd Challenge.LargeFileSort
   dotnet build
   ```

After building, the executables will be available in the `bin\Debug\net8.0` folder of each project.

---

## 1. FileGenerator

### Description

**FileGenerator** creates a test file of a specified size in gigabytes. This is useful for testing sorting algorithms or handling large data files.

### Usage

#### Command-Line Parameters

Navigate to the project folder where the executable is located:
```bash
cd {PathToRepository}\Challenge.LargeFileSort\Challenge.LargeFileSort.Create\bin\Debug\net8.0
```

Run the command:
```bash
Challenge.LargeFileSort.Create.exe <outputFile> <targetSizeGB>
```

- **outputFile**: Path to the output file.
- **targetSizeGB**: Desired size of the generated file in gigabytes.

#### Example with Parameters

```bash
Challenge.LargeFileSort.Create.exe "C:\TestFiles\largeFile.txt" 1.5
```

This command creates a file `largeFile.txt` of size 1.5 GB in the `C:\TestFiles` directory. A log file `largeFile.txt.log` is created alongside the output file, containing detailed information about the generation process.

#### Interactive Mode

If no parameters are provided, **FileGenerator** prompts for input interactively.

```bash
FileGenerator.exe
```

**Program Output:**
```
No parameters were provided. Please input them manually.
Enter the output file path: C:\TestFiles\largeFile.txt
Enter the target file size in gigabytes (GB): 1.5
```

This will create the file as specified interactively. A log file `largeFile.txt.log` is generated in the same directory as the output file.

---

## 2. FileSorter

### Description

**FileSorter** splits a large file into chunks, sorts each chunk using a specified algorithm, and then merges the sorted chunks into an output file.

### Usage

#### Command-Line Parameters

Navigate to the project folder where the executable is located:
```bash
cd {PathToRepository}\Challenge.LargeFileSort\Challenge.LargeFileSort\bin\Debug\net8.0
```

Run the command:
```bash
Challenge.LargeFileSort.exe <inputFile> <outputFile> <algorithm> <chunkSizeMB> [<maxThreads>]
```

- **inputFile**: Path to the input file to be sorted.
- **outputFile**: Path to the output sorted file.
- **algorithm**: Sorting algorithm to use (`QuickSort` or `Timsort`).
- **chunkSizeMB**: Size of each chunk in megabytes.
- **maxThreads** *(optional)*: Maximum number of threads for parallel processing.

#### Example with Parameters

```bash
Challenge.LargeFileSort.exe "C:\TestFiles\largeFile.txt" "C:\TestFiles\sortedFile.txt" QuickSort 128 4
```

This command sorts `largeFile.txt` into chunks of 128 MB each, uses `QuickSort` as the sorting algorithm, and processes the chunks using up to 4 threads. The sorted result is saved as `sortedFile.txt`. A log file `sortedFile.txt.log` is created alongside the output file.

#### Interactive Mode

If no parameters are provided, **FileSorter** prompts for input interactively.

```bash
Challenge.LargeFileSort.exe
```

**Program Output:**
```
Not enough parameters provided. Please input them manually.
Enter the path to the input file: C:\TestFiles\largeFile.txt
Enter the path to the output file: C:\TestFiles\sortedFile.txt
Enter the chunk size in MB: 128
Enter the algorithm (Timsort or QuickSort): QuickSort
Enter the maximum number of threads to use (optional, press Enter to use default): 4
```

This sorts the file based on the inputs provided. A log file `sortedFile.txt.log` is generated in the same directory as the output file.

---

## Additional Notes

1. **Default Values:**

   - If the chunk size is invalid or not provided, it defaults to **128 MB**.
   - If the number of threads is invalid or not provided, it defaults to the system's processor count.
   - If the algorithm is invalid or not provided, it defaults to `QuickSort`.
   - These defaults are logged for reference.

2. **Logs:**

   - Both tools log their operations to a file named after the output file with the `.log` extension (e.g., `largeFile.txt.log`, `sortedFile.txt.log`). Logs include timestamps and details of each step.

3. **Performance Considerations:**

   - Use larger chunk sizes for better performance on high-memory systems.
   - Use fewer threads if running on a machine with limited CPU resources.

---

## Troubleshooting

1. **Input File Not Found:**
   - Ensure the input file path is correct and accessible.
2. **Insufficient Disk Space:**
   - Ensure enough free space is available for temporary and output files.
3. **Log File Review:**
   - Check the generated log file for detailed information on errors or warnings.
