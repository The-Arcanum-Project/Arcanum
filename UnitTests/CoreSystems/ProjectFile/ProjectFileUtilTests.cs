using System.IO.Compression;
using Arcanum.Core.CoreSystems.ProjectFileUtil;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;

namespace UnitTests.CoreSystems.ProjectFile;

[TestFixture]
public class ProjectFileUtilTests
{
   private string _testDir;

   [SetUp]
   public void SetUp()
   {
      _testDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      Directory.CreateDirectory(_testDir);
   }

   [TearDown]
   public void TearDown()
   {
      if (Directory.Exists(_testDir))
         Directory.Delete(_testDir, true);
   }

   [Test]
   public void CreateZipArchive_CreatesValidZip()
   {
      var zipPath = Path.Combine(_testDir, "test.zip");
      using var zip = ProjectFileUtil.CreateZipArchive(zipPath);
      Assert.That(File.Exists(zipPath), Is.True);
   }

   [Test]
   public void AddFileToZip_AddsFileCorrectly()
   {
      var filePath = Path.Combine(_testDir, "test.txt");
      File.WriteAllText(filePath, "Hello");

      var zipPath = Path.Combine(_testDir, "test.zip");
      var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
      ProjectFileUtil.AddFileToZip(zip, filePath);
      zip.Dispose(); // Ensure the zip is closed before checking

      zip = ZipFile.OpenRead(zipPath);
      Assert.That(zip.GetEntry("test.txt"), Is.Not.Null);
      zip.Dispose();
   }

   [Test]
   public void AddFileToZip_ThrowsIfFileMissing()
   {
      using var zip = ZipFile.Open(Path.Combine(_testDir, "test.zip"), ZipArchiveMode.Create);
      Assert.Throws<FileNotFoundException>(() => ProjectFileUtil.AddFileToZip(zip, "missing.txt"));
   }

   [Test]
   public void AddFileFromStringToArchive_WritesContent()
   {
      var zipPath = Path.Combine(_testDir, "test.zip");
      var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
      ProjectFileUtil.AddFileFromStringToArchive(zip, "data.txt", "content123");
      zip.Dispose(); // Ensure the zip is closed before reading

      using var zipRead = ZipFile.OpenRead(zipPath);
      var entry = zipRead.GetEntry("data.txt");
      using var reader = new StreamReader(entry!.Open());
      Assert.That(reader.ReadToEnd(), Is.EqualTo("content123"));
   }

   [Test]
   public void CreateFromFiles_CreatesArchiveWithFiles()
   {
      var file1 = Path.Combine(_testDir, "a.txt");
      var file2 = Path.Combine(_testDir, "b.txt");
      File.WriteAllText(file1, "A");
      File.WriteAllText(file2, "B");

      var output = ProjectFileUtil.CreateFromFiles([file1, file2], "output", _testDir);
      Assert.That(File.Exists(output), Is.True);

      using var zip = ZipFile.OpenRead(output);
      Assert.That(zip.GetEntry("a.txt"), Is.Not.Null);
      Assert.That(zip.GetEntry("b.txt"), Is.Not.Null);
   }

   [Test]
   public void CreateAndRemoveEntries_RemovesFiles()
   {
      var file1 = Path.Combine(_testDir, "x.txt");
      File.WriteAllText(file1, "X");

      var output = ProjectFileUtil.CreateAndRemoveEntries([file1], "deltest", _testDir);
      Assert.That(File.Exists(output), Is.True);
      Assert.That(File.Exists(file1), Is.False);
   }

   [Test]
   public void GetFileFromProject_ReturnsFileContent()
   {
      var zipPath = Path.Combine(_testDir, "proj.arcanum");
      using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
      {
         var entry = zip.CreateEntry("test.txt");
         using var writer = new StreamWriter(entry.Open());
         writer.Write("testdata");
      }

      var extracted = ProjectFileUtil.GetFileFromProject(zipPath, "test.txt");
      Assert.That(File.Exists(extracted), Is.True);
      Assert.That(File.ReadAllText(extracted!), Is.EqualTo("testdata"));
   }

   [Test]
   public void GetFileFromProject_ThrowsOnInvalidFile()
   {
      var path = Path.Combine(_testDir, "notvalid.zip");
      File.WriteAllText(path, "garbage");

      Assert.Throws<InvalidDataException>(() =>
                                             ProjectFileUtil.GetFileFromProject(path, "x.txt"));
   }

   [Test]
   public void ExtractProjectFile_ExtractsAllEntries()
   {
      var zipPath = Path.Combine(_testDir, "proj.arcanum");
      using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
      {
         using (var stream1 = zip.CreateEntry("one.txt").Open())
            stream1.Write([1], 0, 1);
         using (var stream2 = zip.CreateEntry("two.txt").Open())
            stream2.Write([2], 0, 1);
      }

      var extracted = ProjectFileUtil.ExtractProjectFile(zipPath, _testDir);
      Assert.That(extracted, Has.Count.EqualTo(2));
      Assert.That(File.Exists(extracted[0]), Is.True);
   }

   [Test]
   public void GatherFilesForProjectFile_CreatesProjectFile()
   {
      var descriptor = new ProjectFileDescriptor("TestMod", "Path/To/TestMod", "Path/To/Vanilla");
      ProjectFileUtil.GatherFilesForProjectFile(descriptor);

      var zipPath = Path.Combine(Arcanum.Core.CoreSystems.IO.IO.GetArcanumDataPath,
                                 "ArcanumProjects",
                                 "TestMod.arcanum");
      Assert.That(File.Exists(zipPath), Is.True);

      using var zip = ZipFile.OpenRead(zipPath);
      Assert.That(zip.GetEntry("ProjDescriptor.json"), Is.Not.Null);
   }
}