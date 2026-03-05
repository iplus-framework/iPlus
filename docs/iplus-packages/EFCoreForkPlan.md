# Plan za korištenje EFCore forka kao NuGet paketa

1. Izradi NuGet pakete iz svog EFCore forka:
   - Pokreni `dotnet pack -c Release` u rootu forka.
   - Kopiraj .nupkg datoteke u `c:\Devel\iplus-github\packages\ef_90_iPlus\`.

2. U rootu rješenja dodaj/uredi `NuGet.config`:
   - Dodaj lokalni feed:
     ```xml
     <add key="LocalEFCoreFork" value="..\packages\ef_90_iPlus\" />
     ```

3. U `Directory.Packages.props`:
   - Dodaj property za verziju forka:
     ```xml
     <EFCoreForkVersion>9.0.7</EFCoreForkVersion>
     ```
   - Dodaj conditional reference na EFCore pakete:
     ```xml
     <ItemGroup Condition="'$(UseDotNet10)' == 'False' and '$(UseEFCoreForkIPlus)' == 'True'">
       <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="$(EFCoreForkVersion)"/>
       ...
     </ItemGroup>
     ```

4. U `Directory.Build.props`:
   - Dodaj property:
     ```xml
     <UseEFCoreForkIPlus>True</UseEFCoreForkIPlus>
     ```
   - Postavi na `False` za službene pakete.

5. U `build/EntityFramework.Build.props`:
   - Zakomentiraj/ukloni reference na DLL-ove (EFCoreRootDir, EFCoreIncludeDir).

6. Restore/build:
   - Pokreni `dotnet restore` i `dotnet build`.
   - Provjeri da se koriste paketi iz lokalnog feeda.

7. Test:
   - Provjeri da se koristi tvoj fork (verzija, promjene u kodu).

---

Ako radiš novi clone:
- Samo ponovi korake 1-6.
- Sve je automatizirano kroz props/config.
