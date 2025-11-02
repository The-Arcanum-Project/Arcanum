using Arcanum.API.Core.IO;
using Arcanum.Core.CoreSystems.ConsoleServices;

namespace UnitTests;

using Arcanum.API;
using Arcanum.API.Console;
using Moq;
using NUnit.Framework;

[TestFixture]
public class ConsoleServiceImplTests
{
   private Mock<IPluginHost> _pluginHost;
   private Mock<IOutputReceiver> _output;
   private Mock<IJsonProcessor> _json;
   private Mock<IFileOperations> _io;

   private ConsoleServiceImpl _service;

   [SetUp]
   public void Setup()
   {
      _pluginHost = new();
      _output = new();
      _json = new();
      _io = new();

      _pluginHost.Setup(x => x.GetService<IJsonProcessor>()).Returns(_json.Object);
      _pluginHost.Setup(x => x.GetService<IFileOperations>()).Returns(_io.Object);
      _io.Setup(x => x.GetArcanumDataPath).Returns("TestPath");

      _service = new(_pluginHost.Object, "TestConsole", _output.Object, category: DefaultCommands.CommandCategory.All);
   }

   private readonly string[] _testOutput = ["executed TestCommand", "with arguments:", "a, b, c"];

   private ICommandDefinition TestCommandDefinition()
   {
      return new DefaultCommands.DefaultCommandDefinition("test",
                                                          "Test command for unit testing",
                                                          args =>
                                                          {
                                                             var output = new string[_testOutput.Length];
                                                             output[0] = _testOutput[0];
                                                             output[1] = _testOutput[1];
                                                             output[2] = string.Join(", ", args);
                                                             return output;
                                                          },
                                                          ClearanceLevel.Debug,
                                                          DefaultCommands.CommandCategory.Debug,
                                                          "alias1",
                                                          "alias2");
   }

   [Test]
   public void RegisterCommand_StoresCommandAndAliases()
   {
      var cmd = TestCommandDefinition();
      _service.RegisterCommand(cmd);

      Assert.That(_service.GetCommandNames().Contains("test"));

      _service.GetCommandDefinition("test", out var definition);
      Assert.IsNotNull(definition);
      Assert.That(definition.Name, Is.EqualTo("test"));
      Assert.That(definition.Aliases.Contains("alias1"));
      Assert.That(definition.Aliases.Contains("alias2"));
   }

   [Test]
   public void UnRegisterCommand_RemovesCommandAndAliases()
   {
      var cmd = TestCommandDefinition();

      _service.RegisterCommand(cmd);
      _service.UnregisterCommand("test");

      Assert.That(_service.GetCommandNames().Contains("test"), Is.False);
      _service.GetCommandDefinition("test", out var definition);
      Assert.IsNull(definition);
      Assert.That(_service.GetCommandNames().Any(x => x.Equals("alias1", StringComparison.OrdinalIgnoreCase)),
                  Is.False);
      Assert.That(_service.GetCommandNames().Any(x => x.Equals("alias2", StringComparison.OrdinalIgnoreCase)),
                  Is.False);
   }

   [Test]
   public void ProcessCommand_ExecutesCommandAndReturnsOutput()
   {
      var cmd = TestCommandDefinition();
      _service.RegisterCommand(cmd);

      var output = _service.ProcessCommand("test a b c");
      Assert.IsNotNull(output);
      for (var i = 0; i < _testOutput.Length; i++)
         Assert.That(_testOutput[i], Is.EqualTo(output[i]));
   }

   // Macro Tests
   [Test]
   public void AddMacro_StoresMacro()
   {
      var added = _service.AddMacro("key", "value");

      Assert.That(added);
      Assert.That(_service.GetMacros().ContainsKey("key"));
   }

   [Test]
   public void RunMacro_ExecutesMacroIfExists()
   {
      var cmd = TestCommandDefinition();
      _service.RegisterCommand(cmd);
      _service.AddMacro("m1", "test a b c");

      _service.RunMacro("m1", out var output);

      Assert.IsNotNull(output);
      for (var i = 0; i < _testOutput.Length; i++)
         Assert.That(_testOutput[i], Is.EqualTo(output[i]));
   }

   [Test]
   public void RunMacro_OutputsErrorIfMissing()
   {
      _service.RunMacro("missing", out var output);

      Assert.That(output[0], Is.EqualTo("Macro 'missing' not found."));
   }

   [Test]
   public void History_AddsEntry_And_ClearsCorrectly()
   {
      _service.ProcessCommand("test");
      Assert.That(_service.GetHistory(), Has.Count.EqualTo(1));

      _service.ClearHistory();
      Assert.That(_service.GetHistory(), Is.Empty);
   }

   [Test]
   public void SaveMacros_ThrowsIfServicesMissing()
   {
      _pluginHost.Setup(x => x.GetService<IFileOperations>()).Returns((IFileOperations)null!);

      Assert.Throws<InvalidOperationException>(() => _service.SaveMacros());
   }

   [Test]
   public void SaveHistory_ThrowsIfServicesMissing()
   {
      _pluginHost.Setup(x => x.GetService<IJsonProcessor>()).Returns((IJsonProcessor)null!);

      Assert.Throws<InvalidOperationException>(() => _service.SaveHistory());
   }

   [Test]
   public void GetPreviousCommand_ReturnsPreviousCommand()
   {
      _service.ProcessCommand("test1");
      _service.ProcessCommand("test2");

      var previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test2"));

      previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test1"));
   }

   [Test]
   public void GetNextCommand_ReturnsNextCommand()
   {
      _service.ProcessCommand("test1");
      _service.ProcessCommand("test2");

      var previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test2"));

      previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test1"));

      var next = _service.GetNextHistoryEntry();
      Assert.That(next, Is.EqualTo("test2"));

      next = _service.GetNextHistoryEntry();
      Assert.IsEmpty(next);
   }

   [Test]
   public void GetPreviousCommandWhenGoingBackToStart_ReturnsFirstEntry()
   {
      _service.ProcessCommand("test1");
      _service.ProcessCommand("test2");

      var previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test2"));

      previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test1"));

      previous = _service.GetPreviousHistoryEntry();
      Assert.That(previous, Is.EqualTo("test1"), "Should return the first entry when going back to start.");
   }

   // Console Parser
   [Test]
   public void SplitsArguments_ByDefaultSeparator()
   {
      var input = new[] { "a,b,c", "d,e" };

      var result = ConsoleParser.GetSubArguments(input);

      Assert.That(result, Is.EqualTo(new[] { new[] { "a", "b", "c" }, new[] { "d", "e" } }));
   }

   [Test]
   public void HandlesQuotes_IgnoresSeparatorInsideQuotes()
   {
      var input = new[] { "a,\"b,c\",d" };

      var result = ConsoleParser.GetSubArguments(input);

      Assert.That(result, Is.EqualTo(new[] { new[] { "a", "b,c", "d" } }));
   }

   [Test]
   public void SkipsEmptyEntriesBetweenSeparators()
   {
      var input = new[] { "a,,b" };

      var result = ConsoleParser.GetSubArguments(input);

      Assert.That(result, Is.EqualTo(new[] { new[] { "a", "b" } }));
   }

   [Test]
   public void SupportsCustomSeparator()
   {
      var input = new[] { "a;b;c" };

      var result = ConsoleParser.GetSubArguments(input, ';');

      Assert.That(result, Is.EqualTo(new[] { new[] { "a", "b", "c" } }));
   }

   [Test]
   public void HandlesTrailingSeparator()
   {
      var input = new[] { "a,b," };

      var result = ConsoleParser.GetSubArguments(input);

      Assert.That(result, Is.EqualTo(new[] { new[] { "a", "b" } }));
   }

   [Test]
   public void HandlesOnlyQuotesProperly()
   {
      var input = new[] { "\"a,b,c\"" };

      var result = ConsoleParser.GetSubArguments(input);

      Assert.That(result, Is.EqualTo(new[] { new[] { "a,b,c" } }));
   }
}