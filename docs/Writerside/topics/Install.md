# Installation

Arcanum can be installed in two ways:

* **Executable (.exe):** The easiest method. Just download and run — no setup required.
* **Source Code:** For developers or advanced users who want to build Arcanum manually.

Arcanum is fully portable, meaning no installer is required. Place the executable file wherever you like and run it directly.

> Building from source requires basic knowledge of compiling .NET applications. If you just want to use Arcanum, we
> recommend the executable method.
> {style="note"}

<tabs>
    <tab id="executable-install" title="Executable">
      <procedure title="Install using Executable" id="executable-id">
          <step>
            Go to the <a href="https://github.com/The-Arcanum-Project/Arcanum/releases">Releases tab</a>.
          </step>
          <step>
            Download the latest version of the <b>Arcanum executable</b> (<code>Arcanum.exe</code>).
          </step>
          <step>
            Place the file in any folder of your choice (for example, <code>C:\Tools\Arcanum\</code>).
          </step>
          <step>
            Double-click <code>Arcanum.exe</code> to launch.
          </step>
      </procedure>
    </tab>
    <tab id="macos-install" title="Source Code">
      This option is for those who want to build the project themselves.
        <procedure title="Prerequisites">
        <p><a href="https://dotnet.microsoft.com/download">.NET SDK (.NET 8.0)</a></p>
        <p><a href="https://git-scm.com/downloads">Git</a></p>
      </procedure>
      <procedure title="Install from source">
         <step>
           <b>Clone the Repository:</b> Open a terminal/command prompt and run:
           <code-block language="bash">
               git clone https://github.com/The-Arcanum-Project/Arcanum.git
               cd Arcanum
           </code-block>
         </step>
         <step>
           <b>Build the Project:</b> Compile in <b>Release mode</b>:
           <code-block language="bash">
               dotnet build -c Release
           </code-block>
         </step>
         <step>
           <b>Locate the Executable:</b> After a successful build, the compiled executable will be in:
           <code language="bash">
               ./bin/Release/net8.0/Arcanum.exe
           </code>
         <note>The exact folder may vary depending on your .NET version.</note>
         </step>
         <step>
           <b>Run Arcanum:</b> Execute the program with::
           <code-block language="bash">
               ./bin/Release/net8.0/Arcanum.exe
           </code-block>
         </step>
      </procedure>
   </tab>
</tabs>