ArgumentsParser
===========

Parses Command Line Arguments

Arguments Example:
```
public static int Main(string[] rawArgs) {
    var args = new Arguments("This is a sample meant to demonstrate how to use the Args library in app-commons.");
    var verbose = args.AddSwitch("v", "verbose", "Print lots of output.");
    var num = args.Add<int>(
        "n",
        "number",
        "This is a numeric argument.  Passing in a non-number will cause args.IsValid to be false and won't throw an exception.",
        required: true);

    args.Parse(rawArgs);

    if (!args.IsValid)
    {
        Console.Out.WriteLine("Invalid arguments");
        args.PrintErrors(Console.Error);
    }

    if (!args.IsValid || rawArgs.Length == 0)
    {
        args.PrintUsage(Console.Out);
        return 1;
    }

    Console.WriteLine("Arguments are valid.");
}
```

When run, this prints the following output:

```
MyApplication.exe - This is a sample meant to demonstrate how to use the Args library in app-commons.

usage: MyApplication.exe [-v] -n <int>

  v,verbose - Boolean; optional.  Print lots of output.
  n,number - int.  This is a numeric argument.
```