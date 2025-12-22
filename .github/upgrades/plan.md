# .NET 10.0 Upgrade Plan - MeterKloud Solution

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
  - [MeterDataLib.csproj](#meterdatalibcsproj)
  - [Api.csproj](#apicsproj)
  - [MeterKloud.csproj (Client)](#meterkloudcsproj-client)
  - [TestMeterLib.csproj](#testmeterlibcsproj)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description
Upgrade the MeterKloud solution from .NET 9.0 to .NET 10.0 (Long Term Support).

### Scope

**Projects Affected:** 4 projects
- **MeterDataLib.csproj** - Class library (10,204 LOC) - Core meter data parsing library
- **Api.csproj** - Azure Functions (38 LOC) - Serverless API endpoints
- **MeterKloud.csproj** - Blazor WebAssembly (1,117 LOC) - Client-side web application
- **TestMeterLib.csproj** - Test project (2,524 LOC) - Unit/integration tests

**Current State:** All projects targeting net9.0
**Target State:** All projects targeting net10.0

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in a single atomic operation.

**Rationale:**
- **Small solution size:** 4 projects, well under the 30-project threshold
- **Simple dependency structure:** Maximum depth of 2 levels, no circular dependencies
- **Low risk profile:** All projects rated ?? Low difficulty, zero security vulnerabilities
- **Clean compatibility:** No binary/source incompatible APIs, only 4 minor behavioral changes
- **Homogeneous codebase:** All SDK-style projects, consistent .NET 9.0 baseline
- **High package compatibility:** 56.5% of packages already compatible, clear upgrade path for remaining 43.5%

This strategy enables the fastest completion time with minimal coordination overhead. The entire solution can be upgraded, built, and tested as a single coordinated unit.

### Complexity Assessment

**Discovered Metrics:**
- Total Projects: 4
- Total LOC: 13,883
- Dependency Depth: 2 levels
- Circular Dependencies: 0
- Security Vulnerabilities: 0 ?
- Package Updates Required: 10 of 23 packages (43.5%)
- API Compatibility Issues: 0 binary/source incompatible, 4 behavioral changes
- Estimated LOC Impact: 4+ lines (0.03% of codebase)

**Classification: Simple Solution**

All projects exhibit low complexity characteristics:
- Small to medium codebase sizes
- Clear dependency relationships
- SDK-style project files (easy to modify)
- Minimal expected code changes
- Strong test coverage (TestMeterLib.csproj)

### Critical Issues

? **No Blocking Issues Identified**

- No security vulnerabilities
- No incompatible packages
- No binary/source breaking API changes
- All required package versions available

?? **Minor Considerations:**
- 4 behavioral API changes requiring runtime validation (System.Uri, HostBuilder)
- Azure Functions Worker packages require updates (2.x ? 2.51.0/2.50.0)
- Blazor WebAssembly packages need alignment with .NET 10.0

### Recommended Approach

**All-At-Once Migration:**
1. Update all 4 project files to `<TargetFramework>net10.0</TargetFramework>` simultaneously
2. Update all 10 package references to .NET 10.0-compatible versions in one operation
3. Restore dependencies and build entire solution
4. Fix any compilation errors (expected to be minimal)
5. Run full test suite to validate behavioral changes
6. Single commit approach for the entire upgrade

**Expected Timeline:** Single iteration with comprehensive validation

### Iteration Strategy Used

**Fast Batch Approach** - Plan generation in 2-3 iterations:
1. ? Foundation (Iterations 1.1-1.3): Skeleton, classification, strategy
2. ? Foundation completion (Iteration 2.1-2.3): Dependency analysis, migration strategy details, project stubs
3. ? Batched details (Iteration 3.1): All 4 project details, risk management, complexity assessment
4. ? Final polish (Iteration 3.2): Testing strategy, source control, success criteria

This approach leverages the simple solution structure to minimize iteration overhead while maintaining comprehensive planning.

---

## Migration Strategy

### Approach Selection

**Selected Approach: All-At-Once Migration**

**Justification:**

The All-At-Once strategy is ideal for the MeterKloud solution based on the following assessment criteria:

? **Solution Size:** 4 projects (well below the 5-project threshold for simple solutions)

? **Dependency Complexity:** Clean two-tier structure with no circular dependencies

? **Risk Profile:** 
- All projects rated ?? Low difficulty
- Zero security vulnerabilities
- Zero binary/source incompatible APIs
- Only 4 minor behavioral changes (low impact)

? **Codebase Homogeneity:**
- All projects currently on net9.0
- All SDK-style projects (easy to modify)
- Consistent tooling and patterns

? **Package Compatibility:**
- 56.5% already compatible
- Clear upgrade paths for remaining 43.5%
- All required versions available

? **Test Coverage:**
- Dedicated test project (TestMeterLib) with 2,524 LOC
- Can validate entire solution after atomic upgrade

**All-At-Once Strategy Rationale:**

This strategy provides the fastest path to completion while maintaining safety:
- **Simplicity:** No need to manage multi-targeting or intermediate states
- **Speed:** Single coordinated update minimizes total timeline
- **Clean dependencies:** All package versions align to .NET 10.0 simultaneously
- **Atomic validation:** Build and test entire solution as one unit
- **Source control:** Single commit captures entire upgrade (preferred approach)

### Dependency-Based Ordering

**All-At-Once Ordering Principles:**

While all projects are updated simultaneously, the *build and validation* sequence respects dependency order:

1. **Update Phase** (atomic - all at once):
   - All project files: `<TargetFramework>net10.0</TargetFramework>`
   - All package references to .NET 10.0 versions
   - Restore dependencies

2. **Build Validation Sequence** (respects dependencies):
   - MeterDataLib.csproj (foundation, no dependencies)
   - Api.csproj (independent)
   - MeterKloud.csproj (depends on MeterDataLib)
   - TestMeterLib.csproj (depends on MeterDataLib)

3. **Test Execution** (after all builds succeed):
   - Run TestMeterLib.csproj test suite
   - Validate behavioral changes at runtime

**Key Principle:** Updates are simultaneous, but validation follows dependency order to isolate any issues quickly.

### Parallel vs Sequential Execution

**Execution Model: Atomic Update with Sequential Validation**

**Update Operations (Simultaneous):**
- ? All 4 project file modifications in parallel
- ? All 10 package reference updates in parallel
- ? Single `dotnet restore` for entire solution

**Build Operations (Sequential with Dependency Awareness):**
- Build order respects dependencies but executed via `dotnet build MeterKloud.sln`
- MSBuild automatically parallelizes where safe
- Errors isolated by project for faster troubleshooting

**Test Operations (After All Builds Succeed):**
- Run TestMeterLib.csproj as single test suite
- Additional manual validation for behavioral changes (System.Uri, HostBuilder)

**Rationale:**
- Small solution size makes parallel coordination unnecessary
- Sequential validation simplifies error diagnosis
- Single solution build leverages MSBuild's built-in parallelization

### Phase Definitions

**Phase 0: Preparation (if needed)**
- ? Verify .NET 10.0 SDK installed
- ? Check for global.json constraints (none expected based on assessment)
- ? Ensure on correct branch: `upgrade/dotnet-10.0-20250118-173421`

**Phase 1: Atomic Upgrade**

**Operations** (performed as single coordinated batch):
1. Update all project files to `net10.0`
2. Update all package references to .NET 10.0 versions
3. Restore dependencies (`dotnet restore`)
4. Build entire solution (`dotnet build MeterKloud.sln`)
5. Fix any compilation errors (expected to be minimal)
6. Rebuild to verify fixes

**Deliverables:** 
- Solution builds with 0 errors
- All projects target net10.0
- All packages updated per §Package Update Reference

**Phase 2: Test Validation**

**Operations:**
1. Execute TestMeterLib.csproj test suite
2. Address any test failures
3. Manual validation of behavioral changes (System.Uri, HostBuilder usage)

**Deliverables:**
- All tests pass
- Behavioral changes validated
- No runtime regressions

**Phase 3: Final Verification**

**Operations:**
1. Full solution rebuild (clean + build)
2. Review for any warnings
3. Commit changes to source control

**Deliverables:**
- Clean build with no warnings
- All changes committed
- Upgrade complete

### Expected Outcomes

**Success Metrics:**
- ? All 4 projects targeting net10.0
- ? All 10 packages updated to compatible versions
- ? Solution builds with 0 errors
- ? All tests pass (TestMeterLib.csproj)
- ? No new warnings introduced
- ? Behavioral changes validated

**Timeline:**
- Complexity: Low
- Expected smooth execution given clean assessment results
- Single iteration approach (no incremental phases)

---

## Project-by-Project Plans

### MeterDataLib.csproj

**Current State:** 
- Target Framework: net9.0
- Project Type: ClassLibrary (SDK-style)
- LOC: 10,204
- Dependencies: 0 project dependencies
- Dependants: 2 (MeterKloud.csproj, TestMeterLib.csproj)
- Current Packages:
  - ExcelDataReader 3.8.0 (? compatible)
  - Microsoft.Extensions.Logging.Abstractions 9.0.10 (needs update)

**Target State:**
- Target Framework: net10.0
- Package Updates: 1 (Microsoft.Extensions.Logging.Abstractions 9.0.10 ? 10.0.1)

**Migration Steps:**

1. **Prerequisites:**
   - None (leaf node, no project dependencies)
   - .NET 10.0 SDK installed

2. **Framework Update:**
   - File: `MeterDataLib\MeterDataLib.csproj`
   - Change: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates:**

| Package Name | Current Version | Target Version | Reason |
|--------------|----------------|----------------|---------|
| Microsoft.Extensions.Logging.Abstractions | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |

**Implementation:**
```xml
<!-- Update in MeterDataLib.csproj -->
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.1" />
```

4. **Expected Breaking Changes:**
   - ? None identified
   - Zero binary/source incompatible APIs
   - Zero behavioral changes in this project

5. **Code Modifications:**
   - ? None expected
   - Logging abstractions interface remains stable across .NET versions
   - No obsolete API usage detected

6. **Testing Strategy:**
   - **Build Validation:** `dotnet build MeterDataLib\MeterDataLib.csproj`
   - **Dependency Check:** Verify no package conflicts
   - **Downstream Validation:** Ensure TestMeterLib.csproj and MeterKloud.csproj still build after this change
   - **Runtime Testing:** TestMeterLib.csproj test suite will validate parser functionality

7. **Validation Checklist:**
   - [ ] Project file updated to net10.0
   - [ ] Microsoft.Extensions.Logging.Abstractions updated to 10.0.1
   - [ ] `dotnet restore` succeeds
   - [ ] `dotnet build` succeeds with 0 errors
   - [ ] No new warnings introduced
   - [ ] No package dependency conflicts
   - [ ] TestMeterLib.csproj builds (validates downstream compatibility)

**Risk Level:** ?? Low

**Notes:**
- Foundation library for the entire solution
- Minimal changes required
- Strong test coverage via TestMeterLib.csproj
- No breaking API changes expected

---

### Api.csproj

**Current State:**
- Target Framework: net9.0
- Project Type: Azure Functions (SDK-style)
- LOC: 38
- Dependencies: 0 project dependencies
- Dependants: 0
- Current Packages:
  - Microsoft.ApplicationInsights.WorkerService 2.23.0 (? compatible)
  - Microsoft.Azure.Functions.Worker 2.2.0 (needs update)
  - Microsoft.Azure.Functions.Worker.ApplicationInsights 2.0.0 (needs update)
  - Microsoft.Azure.Functions.Worker.Extensions.Http 3.3.0 (? compatible)
  - Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore 2.1.0 (? compatible)
  - Microsoft.Azure.Functions.Worker.Sdk 2.0.6 (needs update)
  - Microsoft.Extensions.Caching.Memory 9.0.10 (needs update)
  - Microsoft.Extensions.Http 9.0.10 (needs update)
  - System.Text.Encodings.Web 9.0.10 (needs update)

**Target State:**
- Target Framework: net10.0
- Package Updates: 6 packages

**Migration Steps:**

1. **Prerequisites:**
   - None (no project dependencies)
   - .NET 10.0 SDK installed
   - Azure Functions Core Tools (for local testing)

2. **Framework Update:**
   - File: `Api\Api.csproj`
   - Change: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates:**

| Package Name | Current Version | Target Version | Reason |
|--------------|----------------|----------------|---------|
| Microsoft.Azure.Functions.Worker | 2.2.0 | 2.51.0 | .NET 10.0 compatibility; **major version jump** |
| Microsoft.Azure.Functions.Worker.ApplicationInsights | 2.0.0 | 2.50.0 | .NET 10.0 compatibility; Application Insights integration |
| Microsoft.Azure.Functions.Worker.Sdk | 2.0.6 | 2.0.7 | Minor SDK update |
| Microsoft.Extensions.Caching.Memory | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| Microsoft.Extensions.Http | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| System.Text.Encodings.Web | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |

**Implementation:**
```xml
<!-- Update in Api\Api.csproj -->
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.51.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="2.50.0" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.7" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.1" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.1" />
<PackageReference Include="System.Text.Encodings.Web" Version="10.0.1" />
```

4. **Expected Breaking Changes:**

**Behavioral Change:**
- **API:** `Microsoft.Extensions.Hosting.HostBuilder`
- **Impact:** Behavioral change in .NET 10.0 host builder configuration
- **Location:** Check Azure Functions startup code (Program.cs, host configuration)
- **Mitigation:** Review host builder patterns, validate Azure Functions triggers and bindings work correctly

**Azure Functions Worker 2.51.0 Considerations:**
- Major version jump from 2.2.0 ? 2.51.0
- Review release notes: https://github.com/Azure/azure-functions-dotnet-worker/releases
- Potential changes:
  - Function isolation model improvements
  - Middleware pipeline changes
  - Dependency injection patterns

5. **Code Modifications:**
   - **Program.cs / Startup.cs:** Host builder configuration patterns
   - **Function triggers and bindings:** Validate HTTP triggers, timer triggers, etc.
   - **Dependency injection:** Ensure service registration patterns still valid
   - **Application Insights:** Validate telemetry collection after ApplicationInsights package update
   - **Middleware:** Check if custom middleware uses updated APIs

**Specific Checks:**
```csharp
// Verify host builder patterns still valid
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => { ... })
    .Build();
```

6. **Testing Strategy:**

**Build Validation:**
- `dotnet build Api\Api.csproj`
- Verify no compilation errors

**Local Testing:**
- Run Azure Functions locally: `func start` (in Api directory)
- Test all HTTP endpoints
- Verify Application Insights telemetry (if configured)

**Runtime Validation:**
- Test function triggers (HTTP, timers, etc.)
- Verify dependency injection works
- Check host builder behavioral changes don't break startup

**Manual Smoke Tests:**
- Call each Azure Function endpoint
- Verify expected responses
- Check logs for errors/warnings

7. **Validation Checklist:**
   - [ ] Project file updated to net10.0
   - [ ] All 6 packages updated to target versions
   - [ ] `dotnet restore` succeeds
   - [ ] `dotnet build` succeeds with 0 errors
   - [ ] No new warnings introduced
   - [ ] Azure Functions start locally (`func start`)
   - [ ] All HTTP endpoints respond correctly
   - [ ] Application Insights telemetry works (if configured)
   - [ ] Host builder configuration validated
   - [ ] No runtime errors in function execution

**Risk Level:** ?? Low-Medium

**Notes:**
- Azure Functions Worker major version jump requires careful validation
- Minimal LOC (38 lines) reduces code change risk
- Behavioral change in HostBuilder needs runtime testing
- Independent project (no project dependencies) - isolated risk

---

### MeterKloud.csproj (Client)

**Current State:**
- Target Framework: net9.0
- Project Type: Blazor WebAssembly / ASP.NET Core (SDK-style)
- LOC: 1,117
- Dependencies: 1 project (MeterDataLib.csproj)
- Dependants: 0
- Current Packages:
  - AnthonyChu.AzureStaticWebApps.Blazor.Authentication 0.0.2-preview (? compatible)
  - Blazored.LocalStorage 4.5.0 (? compatible)
  - Microsoft.AspNetCore.Components.WebAssembly 9.0.10 (needs update)
  - Microsoft.AspNetCore.Components.WebAssembly.Authentication 9.0.10 (needs update)
  - Microsoft.AspNetCore.Components.WebAssembly.DevServer 9.0.10 (needs update)
  - Microsoft.Extensions.Caching.Memory 9.0.10 (needs update)
  - Microsoft.Extensions.Http 9.0.10 (needs update)
  - MudBlazor 8.14.0 (? compatible)
  - Plotly.Blazor 6.0.2 (? compatible)
  - System.Text.Encodings.Web 9.0.10 (needs update)

**Target State:**
- Target Framework: net10.0
- Package Updates: 6 packages

**Migration Steps:**

1. **Prerequisites:**
   - MeterDataLib.csproj upgraded to net10.0 (dependency)
   - .NET 10.0 SDK installed
   - Workload: `dotnet workload install wasm-tools` (if not present)

2. **Framework Update:**
   - File: `Client\MeterKloud.csproj`
   - Change: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates:**

| Package Name | Current Version | Target Version | Reason |
|--------------|----------------|----------------|---------|
| Microsoft.AspNetCore.Components.WebAssembly | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 9.0.10 | 10.0.1 | Development server for .NET 10.0 |
| Microsoft.Extensions.Caching.Memory | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| Microsoft.Extensions.Http | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |
| System.Text.Encodings.Web | 9.0.10 | 10.0.1 | Align with .NET 10.0 framework |

**Implementation:**
```xml
<!-- Update in Client\MeterKloud.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.1" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" Version="10.0.1" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="10.0.1" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="10.0.1" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.1" />
<PackageReference Include="System.Text.Encodings.Web" Version="10.0.1" />
```

4. **Expected Breaking Changes:**

**Behavioral Changes (3 instances):**

**A. System.Uri Constructor:**
- **API:** `System.Uri.#ctor(System.String)`
- **Impact:** Behavioral change in URI parsing/validation
- **Mitigation:** Review all URI construction code, especially:
  - Navigation URLs
  - API endpoint construction
  - External link handling

**B. System.Uri Type:**
- **API:** `System.Uri` (general usage)
- **Impact:** General behavioral changes in URI handling
- **Affected Areas:**
  - NavigationManager.NavigateTo() calls
  - HTTP client base address configuration
  - Route parameter parsing

**C. Third Instance:**
- Another `System.Uri` behavioral change flagged
- Review all URI-related code comprehensively

**Common Blazor WebAssembly Areas Affected:**
- Program.cs: `builder.HostEnvironment.BaseAddress`
- API clients: `HttpClient.BaseAddress`
- Navigation: `NavigationManager.NavigateTo(uri)`
- Authentication: Redirect URIs

5. **Code Modifications:**

**Areas to Review:**

**Program.cs:**
```csharp
// Review URI construction patterns
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) // ? System.Uri behavioral change
});
```

**API Clients:**
```csharp
// Review API endpoint construction
var apiUri = new Uri($"{baseUrl}/api/endpoint"); // ? Validate URI parsing
```

**Navigation:**
```csharp
// Review navigation logic
NavigationManager.NavigateTo("/page"); // ? Check relative/absolute URI handling
```

**Authentication:**
```csharp
// Review authentication redirect URIs
options.ProviderOptions.DefaultAccessTokenScopes.Add("scope");
options.ProviderOptions.RedirectUri = new Uri("..."); // ? System.Uri behavioral change
```

**Specific Checks:**
- Search codebase for `new Uri(` patterns
- Review NavigationManager usage
- Validate HttpClient BaseAddress configurations
- Check authentication redirect configurations

6. **Testing Strategy:**

**Build Validation:**
- `dotnet build Client\MeterKloud.csproj`
- Verify no compilation errors
- Check for obsolete API warnings

**Local Testing:**
- Run Blazor WASM app: `dotnet run --project Client\MeterKloud.csproj`
- Test in browser (typically https://localhost:5001)

**Runtime Validation:**
- **Navigation Testing:**
  - Test all in-app navigation links
  - Verify route parameters work correctly
  - Check external link handling
- **API Communication:**
  - Verify HttpClient calls to backend APIs work
  - Check API endpoint construction
  - Validate authentication flows (if applicable)
- **Authentication:**
  - Test login/logout flows
  - Verify redirect URIs work correctly
  - Check token acquisition
- **UI Rendering:**
  - Verify MudBlazor components render correctly
  - Test Plotly charts (ensure no runtime errors)
  - Check LocalStorage functionality (Blazored.LocalStorage)

**Browser Console:**
- Check for JavaScript errors
- Verify no Blazor framework errors
- Look for URI-related warnings

7. **Validation Checklist:**
   - [ ] Project file updated to net10.0
   - [ ] All 6 packages updated to target versions
   - [ ] Project reference to MeterDataLib.csproj validated
   - [ ] `dotnet restore` succeeds
   - [ ] `dotnet build` succeeds with 0 errors
   - [ ] No new warnings introduced
   - [ ] Application starts locally
   - [ ] All navigation routes work
   - [ ] API calls succeed (HttpClient configuration valid)
   - [ ] Authentication flows work (if configured)
   - [ ] MudBlazor components render correctly
   - [ ] Plotly charts render correctly
   - [ ] LocalStorage works correctly
   - [ ] No browser console errors
   - [ ] System.Uri behavioral changes validated

**Risk Level:** ?? Low-Medium

**Notes:**
- 3 System.Uri behavioral changes require careful runtime validation
- Blazor WebAssembly client-side runtime - thorough browser testing essential
- Dependency on MeterDataLib.csproj - ensure compatible after upgrade
- MudBlazor and Plotly.Blazor already compatible (no updates needed)
- Focus testing on navigation, API communication, and authentication flows

---

### TestMeterLib.csproj

**Current State:**
- Target Framework: net9.0
- Project Type: Test Project / Console App (SDK-style)
- LOC: 2,524
- Dependencies: 1 project (MeterDataLib.csproj)
- Dependants: 0
- Current Packages:
  - coverlet.collector 6.0.4 (? compatible)
  - FluentAssertions 8.8.0 (? compatible)
  - Microsoft.NET.Test.Sdk 18.0.0 (? compatible)
  - xunit 2.9.3 (? compatible)
  - xunit.runner.visualstudio 3.1.5 (? compatible)

**Target State:**
- Target Framework: net10.0
- Package Updates: **0 packages** (all test packages already compatible ?)

**Migration Steps:**

1. **Prerequisites:**
   - MeterDataLib.csproj upgraded to net10.0 (dependency)
   - .NET 10.0 SDK installed

2. **Framework Update:**
   - File: `TestMeterLib\TestMeterLib.csproj`
   - Change: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates:**

? **No package updates required.** All test framework packages are already compatible with .NET 10.0:
- coverlet.collector 6.0.4
- FluentAssertions 8.8.0
- Microsoft.NET.Test.Sdk 18.0.0
- xunit 2.9.3
- xunit.runner.visualstudio 3.1.5

4. **Expected Breaking Changes:**
   - ? None identified
   - Zero behavioral API changes in this project
   - Test framework APIs stable across .NET versions

5. **Code Modifications:**
   - ? None expected
   - xUnit, FluentAssertions, and test SDK maintain backward compatibility
   - Project reference to MeterDataLib.csproj will automatically use net10.0 version after upgrade

6. **Testing Strategy:**

**Build Validation:**
- `dotnet build TestMeterLib\TestMeterLib.csproj`
- Verify project builds against upgraded MeterDataLib.csproj

**Test Execution:**
- Run full test suite: `dotnet test TestMeterLib\TestMeterLib.csproj`
- This validates:
  - MeterDataLib.csproj functionality after framework upgrade
  - No regressions in parser implementations (Nem12, CsvByChannel, CsvPowerPal, etc.)
  - Package compatibility (ExcelDataReader, logging abstractions)

**Critical Test Files (from context):**
- `CsvByChannelTests.cs` - CSV parser validation
- `ParserNEM12.cs` - NEM12 format parser validation
- Other parser tests (SimpleCsvReader, CsvSingleLine7, CsvPowerPal, etc.)

**Expected Test Results:**
- All existing tests should pass
- No new test failures introduced by framework upgrade
- If tests fail, investigate:
  - MeterDataLib.csproj behavioral changes
  - System.Uri usage in parsers (unlikely but possible)
  - File I/O or stream handling changes

7. **Validation Checklist:**
   - [ ] Project file updated to net10.0
   - [ ] Project reference to MeterDataLib.csproj validated
   - [ ] `dotnet restore` succeeds
   - [ ] `dotnet build` succeeds with 0 errors
   - [ ] No new warnings introduced
   - [ ] `dotnet test` runs successfully
   - [ ] All tests pass (no new failures)
   - [ ] Test coverage maintained
   - [ ] Test output shows expected results

**Risk Level:** ?? Low

**Notes:**
- Simplest upgrade in the solution (framework change only, no package updates)
- **Critical validation role:** Test suite validates entire MeterDataLib.csproj after upgrade
- 2,524 LOC of test coverage provides confidence for the upgrade
- Dependency on MeterDataLib.csproj ensures tests run against upgraded version
- Test failures here indicate real issues in MeterDataLib.csproj migration

---

## Risk Management

**Overall Risk Level: ?? Low**

The MeterKloud solution presents minimal upgrade risk due to:
- Small solution size (4 projects)
- Clean dependency structure
- Zero security vulnerabilities
- No binary/source incompatible APIs
- All SDK-style projects
- Strong test coverage

### High-Risk Changes

**None identified.** All projects rated Low difficulty.

### Medium-Risk Changes

**Behavioral API Changes:**
| Project | API | Risk | Description | Mitigation |
|---------|-----|------|-------------|------------|
| Api.csproj | `Microsoft.Extensions.Hosting.HostBuilder` | ?? Medium | Behavioral change in .NET 10.0 | Review Azure Functions startup/configuration code; validate with runtime testing |
| MeterKloud.csproj | `System.Uri` constructor | ?? Medium | Behavioral change in URI parsing | Review URI construction patterns; run test suite to catch regressions |
| MeterKloud.csproj | `System.Uri` type | ?? Medium | General behavioral changes | Comprehensive testing of URL/navigation scenarios |

**Azure Functions Package Updates:**
| Package | Current | Target | Risk | Mitigation |
|---------|---------|--------|------|------------|
| Microsoft.Azure.Functions.Worker | 2.2.0 | 2.51.0 | ?? Medium | Major version jump; review release notes for breaking changes |
| Microsoft.Azure.Functions.Worker.ApplicationInsights | 2.0.0 | 2.50.0 | ?? Medium | Major version jump; validate Application Insights integration |

### Low-Risk Changes

**Framework Updates:**
- All projects: net9.0 ? net10.0 (incremental .NET version, minimal breaking changes)

**Package Updates (Low Risk):**
- ASP.NET Core packages: 9.0.10 ? 10.0.1 (align with framework)
- Microsoft.Extensions packages: 9.0.10 ? 10.0.1 (align with framework)
- All test packages already compatible

### Security Vulnerabilities

? **No security vulnerabilities identified in current packages.**

No CVEs or security advisories require immediate remediation.

### Contingency Plans

**If Compilation Errors Occur:**
1. Isolate by project using build order (MeterDataLib ? Api ? MeterKloud ? TestMeterLib)
2. Consult .NET 10.0 breaking changes documentation
3. Use `upgrade_get_type_info`, `upgrade_get_member_info` tools for API guidance
4. Review package release notes for breaking changes

**If Behavioral Changes Cause Issues:**
1. System.Uri issues: Review URI construction, test navigation/API calls
2. HostBuilder issues: Review Azure Functions Program.cs/Startup.cs patterns
3. Leverage TestMeterLib.csproj for regression detection
4. Add targeted tests for changed behaviors

**If Azure Functions Worker Updates Cause Issues:**
1. Review Azure Functions Worker 2.51.0 release notes
2. Check for host.json configuration changes
3. Validate function triggers and bindings
4. Test locally with Azure Functions Core Tools before deployment

**If Tests Fail:**
1. Distinguish between test framework issues vs. actual regressions
2. All test packages are compatible, so failures likely indicate real behavioral changes
3. Update tests if behavior change is expected and acceptable
4. Fix code if behavior change is unintended regression

**Rollback Plan:**
- All changes in single commit on dedicated branch `upgrade/dotnet-10.0-20250118-173421`
- Easy rollback: switch back to `net10` branch
- No intermediate states to manage

---

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Dependencies | Package Updates | API Issues | Risk | Rationale |
|---------|-----------|--------------|-----------------|------------|------|-----------|
| MeterDataLib.csproj | ?? Low | 0 projects | 1 | 0 | Low | Foundation library; single package update; no API issues; straightforward upgrade |
| Api.csproj | ?? Low | 0 projects | 5 | 1 | Low-Medium | Minimal LOC (38); Azure Functions Worker updates need validation; 1 behavioral change |
| MeterKloud.csproj | ?? Low | 1 project | 6 | 3 | Low-Medium | Blazor WebAssembly; 3 behavioral changes (System.Uri); moderate package updates |
| TestMeterLib.csproj | ?? Low | 1 project | 0 | 0 | Low | No package updates needed; provides validation for other projects |

### Phase Complexity Assessment

**All-At-Once Strategy - Single Phase:**

**Phase 1: Atomic Upgrade**
- **Complexity:** ?? Low
- **Scope:** All 4 projects, 10 package updates, 4 project file changes
- **Dependencies:** Respect build order (MeterDataLib first, then consumers)
- **Expected Challenges:** 
  - Azure Functions Worker package major version jump (2.2.0 ? 2.51.0)
  - System.Uri behavioral changes in Blazor client
  - HostBuilder behavioral changes in Azure Functions
- **Mitigation:** Comprehensive testing after build succeeds

**Phase 2: Test Validation**
- **Complexity:** ?? Low
- **Scope:** Run TestMeterLib.csproj suite, manual behavioral validation
- **Expected Challenges:** Potential test failures due to System.Uri behavioral changes
- **Mitigation:** Existing test suite provides safety net; update tests if behavior change is expected

### Relative Complexity Ratings

**Overall Solution:** ?? Low Complexity

**Factors Contributing to Low Complexity:**
- ? Small solution (4 projects, <14k LOC)
- ? Clean dependencies (no circular, max depth 2)
- ? All SDK-style projects (easy to modify)
- ? High package compatibility (56.5% already compatible)
- ? No binary/source incompatible APIs
- ? Strong test coverage (TestMeterLib.csproj)
- ? Zero security vulnerabilities
- ? All projects currently on same framework (net9.0)

**Factors Requiring Attention:**
- ?? Azure Functions Worker major version updates (validate thoroughly)
- ?? 4 behavioral API changes (System.Uri, HostBuilder) - runtime validation needed
- ?? Blazor WebAssembly specific considerations (client-side runtime)

### Resource Requirements

**Skills Needed:**
- .NET SDK and project file modification (basic)
- NuGet package management (basic)
- Azure Functions development (for Api.csproj validation)
- Blazor WebAssembly development (for MeterKloud.csproj validation)
- xUnit test framework (for test execution)

**Parallel Capacity:**
- All-At-Once strategy: Single executor sufficient
- No parallel work required (atomic update)
- Sequential validation for clarity

**Tooling:**
- .NET 10.0 SDK (must be installed)
- Visual Studio 2022 or JetBrains Rider (or VS Code with C# extension)
- Azure Functions Core Tools (for local testing of Api.csproj)
- Git (for source control)

### Effort Distribution

**By Phase:**
- Phase 0 (Preparation): Minimal - SDK verification
- Phase 1 (Atomic Upgrade): Low - 4 project files, 10 package updates, build + fix
- Phase 2 (Test Validation): Low - Run existing test suite, validate behaviors
- Phase 3 (Final Verification): Minimal - Clean build, commit

**By Project:**
- MeterDataLib.csproj: Low effort (1 package, no API issues)
- Api.csproj: Low-Medium effort (5 packages, 1 behavioral change, validate Azure Functions)
- MeterKloud.csproj: Low-Medium effort (6 packages, 3 behavioral changes, validate Blazor)
- TestMeterLib.csproj: Minimal effort (no updates, run tests)

**Note:** No time estimates provided. Complexity ratings (Low/Medium/High) indicate relative effort, not absolute duration.

---

## Testing & Validation Strategy

### Phase-by-Phase Testing Requirements

**All-At-Once Strategy: Comprehensive Testing After Atomic Upgrade**

### Phase 1: Build Validation (Immediately After Update)

**Objective:** Verify all projects build successfully with .NET 10.0 and updated packages.

**Build Sequence** (respects dependencies):
1. `dotnet restore MeterKloud.sln` (restore all projects)
2. `dotnet build MeterKloud.sln --no-restore`

**Expected Results:**
- ? All 4 projects build successfully
- ? Zero compilation errors
- ? Zero package dependency conflicts

**If Errors Occur:**
- Isolate by building projects individually in dependency order:
  1. `dotnet build MeterDataLib\MeterDataLib.csproj`
  2. `dotnet build Api\Api.csproj`
  3. `dotnet build Client\MeterKloud.csproj`
  4. `dotnet build TestMeterLib\TestMeterLib.csproj`
- Identify which project has issues
- Fix compilation errors (consult §Project-by-Project Plans for expected changes)
- Rebuild solution

### Phase 2: Automated Test Execution

**Objective:** Validate functionality through existing test suite.

**Test Execution:**
```bash
dotnet test TestMeterLib\TestMeterLib.csproj --no-build --verbosity normal
```

**Test Coverage Areas:**
- CSV parser validation (CsvByChannelTests.cs)
- NEM12 format parser (ParserNEM12.cs)
- Other parser implementations (SimpleCsvReader, CsvSingleLine7, CsvPowerPal)
- MeterDataLib.csproj core functionality

**Expected Results:**
- ? All tests pass
- ? No new test failures introduced
- ? Test execution completes without errors

**If Tests Fail:**
- Review test failure messages
- Distinguish between:
  - **Test framework issues:** (unlikely - all packages compatible)
  - **Actual regressions:** Behavioral changes in MeterDataLib.csproj
- Investigate System.Uri behavioral changes (if URI parsing in parsers affected)
- Fix code or update tests as appropriate
- Re-run tests until all pass

### Phase 3: Runtime Validation (Manual Testing)

**Objective:** Validate behavioral changes and runtime scenarios not covered by automated tests.

**A. Azure Functions (Api.csproj)**

**Local Testing:**
```bash
cd Api
func start
```

**Validation Steps:**
1. ? Azure Functions host starts without errors
2. ? All HTTP endpoints respond correctly
3. ? Function triggers work (HTTP, timers, etc.)
4. ? Dependency injection resolves services
5. ? Application Insights telemetry collected (if configured)

**Specific Checks:**
- HostBuilder configuration (behavioral change area)
- Middleware pipeline execution
- HTTP request/response handling

**B. Blazor WebAssembly (MeterKloud.csproj)**

**Local Testing:**
```bash
cd Client
dotnet run
```
Open browser to https://localhost:5001 (or displayed URL)

**Validation Steps:**
1. ? Application loads in browser
2. ? Navigation works (all routes accessible)
3. ? API communication successful (HttpClient calls)
4. ? Authentication flows work (login/logout if configured)
5. ? MudBlazor components render correctly
6. ? Plotly charts display properly
7. ? LocalStorage functionality works (Blazored.LocalStorage)
8. ? No JavaScript errors in browser console
9. ? No Blazor framework errors

**System.Uri Behavioral Change Focus:**
- Test all navigation links (internal routing)
- Test API endpoint construction
- Test authentication redirect URIs (if configured)
- Verify `new Uri(...)` patterns work correctly
- Check `HttpClient.BaseAddress` configuration
- Validate `NavigationManager.NavigateTo()` calls

**Browser Console Check:**
- Open Developer Tools (F12)
- Check Console tab for errors/warnings
- Monitor Network tab for failed requests

**C. Class Library (MeterDataLib.csproj)**

**Indirect Validation:**
- ? Validated through TestMeterLib.csproj test suite
- ? Validated through MeterKloud.csproj runtime usage

### Smoke Tests (Quick Validation)

**After Each Project Builds:**
- ? No compilation errors
- ? No package restore warnings
- ? No dependency conflicts

**After Solution Builds:**
- ? All 4 projects build together
- ? Clean build output (no errors/warnings)

**After Tests Run:**
- ? All tests pass
- ? Test execution time reasonable (no significant slowdown)

### Comprehensive Validation (Before Completion)

**Final Checklist:**
**Build Quality:**
- [ ] Clean build: `dotnet clean && dotnet build MeterKloud.sln`
- [ ] Zero errors, zero warnings (or only pre-existing warnings)
- [ ] All projects target net10.0 (verify .csproj files)
- [ ] All packages at target versions (verify .csproj files)

**Test Quality:**
- [ ] All automated tests pass (`dotnet test`)
- [ ] Test coverage maintained (no tests skipped/disabled)
- [ ] No new test failures introduced

**Runtime Quality:**
- [ ] Api.csproj: Azure Functions start and respond correctly
- [ ] MeterKloud.csproj: Blazor app loads and functions correctly
- [ ] No runtime exceptions during smoke testing
- [ ] Behavioral changes validated (System.Uri, HostBuilder)

**Code Quality:**
- [ ] No obsolete API warnings (or acceptable warnings documented)
- [ ] No new analyzer warnings
- [ ] Code compiles cleanly

**Package Quality:**
- [ ] No package downgrade warnings
- [ ] No package conflict warnings
- [ ] All packages compatible with net10.0

**Documentation:**
- [ ] Any behavioral changes documented
- [ ] Breaking changes (if any) noted
- [ ] Migration completed per plan

### Performance Validation (Optional)

**If performance is critical:**
- Baseline: Run performance tests on net9.0 version
- Post-upgrade: Run same tests on net10.0 version
- Compare: Ensure no significant regressions
- **Note:** .NET 10.0 typically includes performance improvements

### Regression Testing Scope

**Areas Requiring Extra Attention:**
1. **System.Uri Usage:**
   - Navigation URLs
   - API endpoints
   - Authentication redirects
   - Any URI construction/parsing

2. **HostBuilder Configuration:**
   - Azure Functions startup
   - Service registration
   - Middleware configuration

3. **Blazor WebAssembly Runtime:**
   - Client-side rendering
   - JavaScript interop (if used)
   - WebAssembly specific features

4. **Parser Functionality:**
   - CSV parsing (CsvByChannel, SimpleCsvReader, CsvSingleLine7, CsvPowerPal)
   - NEM12 parsing
   - Excel parsing (ExcelDataReader)
   - File I/O and stream handling

---

## Source Control Strategy

**Branch Strategy:**

- **Source Branch:** `net10` (current development branch)
- **Upgrade Branch:** `upgrade/dotnet-10.0-20250118-173421` (dedicated upgrade branch)
- **Main Branch:** `main` or `master` (production baseline)

**Branching Workflow:**
1. **Start from:** `net10` branch (current branch)
2. **Create upgrade branch:** `upgrade/dotnet-10.0-20250118-173421`
3. **Perform all upgrade work** in dedicated branch
4. **Merge back to:** `net10` after validation complete
5. **Eventually merge to main** following normal release process

**Commit Strategy:**
**All-At-Once Strategy: Single Commit Approach (Preferred)**

All upgrade changes should be captured in **one atomic commit** to maintain clarity and enable easy rollback:

**Single Commit Contents:**
- All 4 project file changes (TargetFramework updates)
- All 10 package version updates
- Any compilation error fixes
- Any behavioral change adaptations

**Commit Message Format:**
```
chore: Upgrade solution from .NET 9.0 to .NET 10.0

- Update all 4 projects to net10.0
- Update 10 NuGet packages to .NET 10.0 compatible versions
- Fix compilation errors from framework upgrade
- Validate behavioral changes (System.Uri, HostBuilder)

Projects updated:
- MeterDataLib.csproj
- Api.csproj (Azure Functions)
- MeterKloud.csproj (Blazor WebAssembly)
- TestMeterLib.csproj

Package updates:
- Microsoft.AspNetCore.Components.WebAssembly 9.0.10 ? 10.0.1
- Microsoft.Azure.Functions.Worker 2.2.0 ? 2.51.0
- Microsoft.Extensions.* 9.0.10 ? 10.0.1
- (see plan.md for complete list)

Validated:
- All projects build successfully
- All tests pass (TestMeterLib.csproj)
- Azure Functions start and respond correctly
- Blazor WebAssembly app renders and navigates correctly

References: assessment.md, plan.md
```

**Alternative: Multi-Commit Approach (if issues require iteration)**

If unexpected issues require multiple fix attempts, use structured commits:

1. **Commit 1:** Framework and package updates
   ```
   chore(upgrade): Update all projects to net10.0 and package versions
   ```

2. **Commit 2:** Compilation error fixes
   ```
   fix(upgrade): Resolve compilation errors from .NET 10.0 migration
   ```

3. **Commit 3:** Behavioral change adaptations
   ```
   fix(upgrade): Address System.Uri and HostBuilder behavioral changes
   ```

4. **Commit 4:** Test fixes (if needed)
   ```
   fix(upgrade): Update tests for .NET 10.0 behavioral changes
   ```

**Commit Checkpoints:**
After each commit, ensure:
- [ ] Solution builds successfully
- [ ] Commit message clearly describes changes
- [ ] Changes are logically grouped
- [ ] No unrelated changes included

**Review and Merge Process:**

**Pull Request Requirements:**
- **Title:** "Upgrade MeterKloud solution to .NET 10.0"
- **Description:** Link to plan.md and assessment.md, summarize changes, list validation steps completed
- **Checklist in PR:**
  - [ ] All 4 projects updated to net10.0
  - [ ] All 10 packages updated per plan
  - [ ] Solution builds with 0 errors
  - [ ] All tests pass (TestMeterLib.csproj)
  - [ ] Azure Functions validated locally
  - [ ] Blazor app validated in browser
  - [ ] Behavioral changes tested (System.Uri, HostBuilder)
  - [ ] No new warnings introduced
  - [ ] plan.md and assessment.md reviewed

**Review Criteria:**
- ? All validation checklist items completed
- ? Project files correctly updated (TargetFramework, PackageReference versions)
- ? No unintended changes outside upgrade scope
- ? Commit history clear and logical
- ? Build and test evidence provided (screenshots or logs)

**Merge Strategy:**
- **Merge method:** Squash and merge (if multi-commit), or regular merge (if single commit)
- **Delete branch:** After successful merge to `net10`
- **Tag release:** Consider tagging after merge: `v1.0.0-net10.0` or similar

**Rollback Procedure (if needed):**

If critical issues discovered post-merge:

1. **Immediate:** Revert merge commit on `net10` branch
2. **Recreate upgrade branch:** From `net10` (pre-upgrade state)
3. **Fix issues** in new upgrade branch
4. **Re-validate** per plan
5. **Re-submit PR** with fixes

**Source Control Best Practices:**
- ? Work in dedicated upgrade branch (isolated from ongoing development)
- ? Commit frequently during development (save progress)
- ? Squash or organize commits before final PR (clean history)
- ? Include plan.md and assessment.md in repository (documentation)
- ? Tag stable points (before upgrade, after upgrade)
- ? Never force-push to shared branches
- ? Keep upgrade branch updated with source branch if development continues

---

## Success Criteria

### Technical Criteria

**Framework Migration:**
- ? All 4 projects target `net10.0` in .csproj files
- ? MeterDataLib.csproj targets net10.0
- ? Api.csproj targets net10.0
- ? MeterKloud.csproj targets net10.0
- ? TestMeterLib.csproj targets net10.0

**Package Updates:**
- ? All 10 required packages updated to .NET 10.0 compatible versions
- ? Microsoft.AspNetCore.Components.WebAssembly: 9.0.10 ? 10.0.1
- ? Microsoft.AspNetCore.Components.WebAssembly.Authentication: 9.0.10 ? 10.0.1
- ? Microsoft.AspNetCore.Components.WebAssembly.DevServer: 9.0.10 ? 10.0.1
- ? Microsoft.Azure.Functions.Worker: 2.2.0 ? 2.51.0
- ? Microsoft.Azure.Functions.Worker.ApplicationInsights: 2.0.0 ? 2.50.0
- ? Microsoft.Azure.Functions.Worker.Sdk: 2.0.6 ? 2.0.7
- ? Microsoft.Extensions.Caching.Memory: 9.0.10 ? 10.0.1
- ? Microsoft.Extensions.Http: 9.0.10 ? 10.0.1
- ? Microsoft.Extensions.Logging.Abstractions: 9.0.10 ? 10.0.1
- ? System.Text.Encodings.Web: 9.0.10 ? 10.0.1
- ? No package downgrade warnings
- ? No package conflict errors

**Build Quality:**
- ? `dotnet restore MeterKloud.sln` succeeds without errors
- ? `dotnet build MeterKloud.sln` succeeds with 0 compilation errors
- ? Clean build produces no new warnings (or acceptable warnings documented)
- ? All projects build individually without errors

**Test Quality:**
- ? `dotnet test TestMeterLib.csproj` passes all tests
- ? No new test failures introduced by migration
- ? Test coverage maintained (no tests skipped or disabled)
- ? Test execution completes without errors

**Runtime Validation:**
- ? Api.csproj: Azure Functions start locally (`func start`)
- ? Api.csproj: All HTTP endpoints respond correctly
- ? Api.csproj: No HostBuilder behavioral change issues
- ? MeterKloud.csproj: Blazor WebAssembly app loads in browser
- ? MeterKloud.csproj: Navigation works correctly (no System.Uri issues)
- ? MeterKloud.csproj: API communication successful
- ? MeterKloud.csproj: MudBlazor and Plotly components render correctly
- ? No JavaScript errors in browser console
- ? No runtime exceptions during smoke testing

**Security:**
- ? No security vulnerabilities introduced by upgrade
- ? No security vulnerabilities remain (started with 0, maintain 0)

### Quality Criteria

**Code Quality:**
- ? No obsolete API usage warnings (or acceptable warnings documented)
- ? No new code analyzer warnings
- ? Code follows existing patterns and conventions
- ? No unintended code changes outside upgrade scope

**Documentation:**
- ? assessment.md accurately reflects pre-upgrade state
- ? plan.md accurately describes upgrade execution
- ? Any behavioral changes documented in code comments (if needed)
- ? Breaking changes documented (if any)
- ? README updated with .NET 10.0 requirements (if applicable)

**Test Coverage:**
- ? Existing test coverage maintained
- ? No tests removed or disabled without justification
- ? Tests updated if behavioral changes require it
- ? New tests added for behavioral change validation (if appropriate)

### Process Criteria

**All-At-Once Strategy Compliance:**
- ? All projects upgraded simultaneously (atomic operation)
- ? No multi-targeting required (clean net10.0 targets)
- ? Single coordinated package update
- ? Unified validation after all updates complete

**Source Control:**
- ? All changes on dedicated upgrade branch: `upgrade/dotnet-10.0-20250118-173421`
- ? Commit history clear and logical (preferably single atomic commit)
- ? Commit messages follow format guidelines
- ? Pull request created with complete validation checklist
- ? Code review completed (if applicable)
- ? Merged back to `net10` branch after approval

**Validation Completion:**
- ? All items in §Testing & Validation Strategy completed
- ? Build validation passed (Phase 1)
- ? Automated test execution passed (Phase 2)
- ? Runtime validation passed (Phase 3)
- ? Comprehensive validation checklist 100% complete
- ? Smoke tests passed
- ? Regression testing completed for identified areas

**Documentation:**
- ? Upgrade process documented (this plan)
- ? Validation results recorded
- ? Any issues encountered and resolutions documented
- ? Lessons learned captured (if applicable)

### Completion Definition

**The .NET 10.0 upgrade is considered COMPLETE when:**
1. ? **All Technical Criteria met** (100% of framework, package, build, test, runtime criteria)
2. ? **All Quality Criteria met** (code quality, documentation, test coverage maintained)
3. ? **All Process Criteria met** (strategy followed, source control proper, validation complete)
4. ? **Sign-off obtained** (if applicable: lead developer, architect, or team approval)
5. ? **Upgrade branch merged** to `net10` branch
6. ? **No outstanding blockers** (all critical issues resolved)

**Acceptance Gates:**
Before declaring completion, verify:
- [ ] Can build solution from clean state (`dotnet clean && dotnet build`)
- [ ] Can run all tests from clean state (`dotnet test`)
- [ ] Can run Azure Functions locally (Api.csproj)
- [ ] Can run Blazor app locally (MeterKloud.csproj)
- [ ] No errors in browser console when using Blazor app
- [ ] All behavioral changes validated (System.Uri, HostBuilder)
- [ ] Code review approved (if process requires it)
- [ ] All documentation updated

**Post-Completion Activities:**
After upgrade complete:
1. ? Update deployment pipelines (if needed for .NET 10.0)
2. ? Update CI/CD configurations (Docker images, build agents, etc.)
3. ? Notify team of completion and any behavioral changes to watch
4. ? Monitor production deployment (if applicable)
5. ? Archive assessment.md and plan.md for future reference

---

**END OF PLAN**
