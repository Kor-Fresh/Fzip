using Fzip.Core;
using System.IO.Compression;
using System.Security.Cryptography;

var root = Path.GetFullPath(args.ElementAtOrDefault(0) ?? ".");
var scratch = Path.Combine(root, "artifacts", "tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(scratch);
var engine = new ArchiveService(Path.Combine(root, "vendor", "7zip", "7z.exe"));
var failures = new List<string>(); int passed = 0;
async Task Test(string name, Func<Task> test)
{
    try { await test(); passed++; Console.WriteLine("PASS " + name); }
    catch (Exception e) { failures.Add(name); Console.WriteLine("FAIL " + name + ": " + e); }
}
void Assert(bool ok, string message) { if (!ok) throw new Exception(message); }
async Task Reject(Func<Task> action)
{
    try { await action(); } catch (Exception e) when (e is IOException or InvalidDataException or ArgumentException or OperationCanceledException) { return; }
    throw new Exception("Expected rejection");
}
var input = Path.Combine(scratch, "input"); Directory.CreateDirectory(input);
Directory.CreateDirectory(Path.Combine(input, "비어 있는 폴더"));
await File.WriteAllTextAsync(Path.Combine(input, "한글 파일.txt"), "Fzip 테스트\nHello archive!");
await File.WriteAllBytesAsync(Path.Combine(input, "binary.dat"), RandomNumberGenerator.GetBytes(150000));
foreach (var format in new[] { "zip", "7z" })
    await Test(format + " roundtrip / Korean / empty folder", async () =>
    {
        var archive = Path.Combine(scratch, "test." + format);
        await engine.Compress(new("compress", Destination: archive, Inputs: [input], Format: format), _ => { }, default);
        var items = await engine.List(archive, default); Assert(items.Any(x => x.Path.Contains("한글")), "Korean name missing");
        var output = Path.Combine(scratch, "out-" + format);
        await engine.Extract(archive, output, _ => { }, default);
        foreach (var file in Directory.EnumerateFiles(input))
        {
            var actual = Path.Combine(output, "input", Path.GetFileName(file));
            Assert(File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(actual)), "Content mismatch");
        }
        Assert(Directory.Exists(Path.Combine(output, "input", "비어 있는 폴더")), "Empty directory missing");
    });
await Test("zip traversal blocked before output", async () =>
{
    var zip = Path.Combine(scratch, "traversal.zip");
    using (var a = ZipFile.Open(zip, ZipArchiveMode.Create)) { using var w = new StreamWriter(a.CreateEntry("../escaped.txt").Open()); w.Write("bad"); }
    var output = Path.Combine(scratch, "traversal");
    await Reject(() => engine.Extract(zip, output, _ => { }, default));
    Assert(!File.Exists(Path.Combine(scratch, "escaped.txt")) && !Directory.Exists(output), "Traversal wrote output");
});
await Test("ambiguous / reserved paths blocked", () =>
{
    foreach (var path in new[] { "C:\\bad", "\\\\server\\x", "file:stream", "folder/../x", "NUL.txt", "CON", "dir./x", "bad\nname" })
    {
        bool blocked = false; try { PathPolicy.Resolve(scratch, path); } catch (InvalidDataException) { blocked = true; }
        Assert(blocked, path + " not rejected");
    }
    bool duplicate = false; try { PathPolicy.ValidateItems(scratch, [new("A", 0, 0, false), new("a", 0, 0, false)]); } catch (InvalidDataException) { duplicate = true; }
    Assert(duplicate, "case collision"); return Task.CompletedTask;
});
await Test("existing output preserved", async () =>
{
    var output = Path.Combine(scratch, "keep"); Directory.CreateDirectory(output);
    var keep = Path.Combine(output, "keep.txt"); await File.WriteAllTextAsync(keep, "original");
    await Reject(() => engine.Extract(Path.Combine(scratch, "test.zip"), output, _ => { }, default));
    Assert(await File.ReadAllTextAsync(keep) == "original", "Existing output modified");
});
await Test("cancelled compression has no published output", async () =>
{
    using var cts = new CancellationTokenSource(); cts.Cancel(); var output = Path.Combine(scratch, "cancelled.zip");
    await Reject(() => engine.Compress(new("compress", Destination: output, Inputs: [input]), _ => { }, cts.Token));
    Assert(!File.Exists(output), "Cancelled output exists");
});
await Test("unknown/corrupt archive rejected", async () =>
{
    var path = Path.Combine(scratch, "bad.zip"); await File.WriteAllTextAsync(path, "not an archive");
    await Reject(() => engine.List(path, default));
});
foreach (var name in new[] { "defaults.alz", "defaults.egg", "small_kor.egg", "solid.egg", "solid_defaults.egg", "azoshort_txt.egg", "basic_deflate_folders.egg", "number.vol1.egg", "Rar.rar", "Arj.method1.arj", "Arj.store.arj" })
    await Test("Archive fixture " + name + " + integrity", async () =>
    {
        var archive = Path.Combine(root, "tests", "fixtures", name);
        var list = await engine.List(archive, default); Assert(list.Count > 0, "No entries");
        var output = Path.Combine(scratch, name + "-out");
        await engine.Extract(archive, output, _ => { }, default);
        Assert(Directory.Exists(output), "No result directory");
        foreach (var item in list.Where(x => !x.IsDirectory)) Assert(new FileInfo(PathPolicy.Resolve(output, item.Path)).Length == item.Size, "Size mismatch");
    });
Console.WriteLine($"RESULT {passed} passed / {failures.Count} failed. Artifacts: {scratch}");
return failures.Count == 0 ? 0 : 1;
