using System.Globalization;
using System.IO;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Saving;

public class WindowsFileNameValidation() : ValidationRule
{
    public FileDescriptor? Descriptor { get; init; } = null;
    
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var filename = value as string;

        if (string.IsNullOrWhiteSpace(filename))
            return new(false, "Filename cannot be empty.");

        var invalidChars = Path.GetInvalidFileNameChars();
        if (filename.Any(c => invalidChars.Contains(c)))
            return new(false,
                $"Filename contains invalid characters: {string.Join(" ", filename.Where(c => invalidChars.Contains(c)))}");

        // TODO @MelCo: Check if the filename was already added in action to the descriptor's files.'
        if (Descriptor is not null && Descriptor.Files.Any(f => f.Path.FilenameWithoutExtension == filename))
            return new(false, "Filename already exists.");
        
        var reservedNames = new []
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };
        return reservedNames.Contains(filename.ToUpperInvariant())
            ? new(false, "Filename is reserved and cannot be used.")
            : ValidationResult.ValidResult;
    }
}