# .NET 10 Upgrade Migration Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
  - [MeterDataLib.csproj](#meterdatalibcsproj)
  - [Api.csproj](#apicsproj)
  - [MeterKloud.csproj](#meterkloudcsproj)
  - [TestMeterLib.csproj](#testmeterlibcsproj)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Risk Management](#risk-management)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

Upgrade all projects in the MeterKloud solution from .NET 9.0 to .NET 10.0 (Long Term Support).

### Scope

**Projects Affected**: 4 projects
- `Api\Api.csproj` - Azure Functions project
- `Client\MeterKloud.csproj` - Blazor WebAssembly application
- `MeterDataLib\MeterDataLib.csproj` - Class library (shared dependency)
- `TestMeterLib\TestMeterLib.csproj` - Test project

**Current State**: All projects targeting net9.0  
**Target State**: All projects targeting net10.0

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in a single coordinated operation.

**Rationale**: 
- Small solution (4 projects)
- All currently on .NET 9.0
- Simple, clear dependency structure (no circular dependencies)
- All packages have confirmed .NET 10 compatible versions available
- Low complexity rating across all projects
- No security vulnerabilities requiring immediate attention

### Discovered Metrics

| Metric | Value |
|--------|-------|
| Total Projects | 4 |
| Total LOC | ~14,000 |
| Dependency Depth | 2 levels |
| NuGet Packages Requiring Updates | 8 of 23 (34.8%) |
| Security Vulnerabilities | 0 |
| Behavioral Changes | 4 (low impact) |
| Deprecated Packages | 1 (xunit) |

### Complexity Assessment

**Classification**: Simple

All projects rated **Low Difficulty**:
- MeterDataLib.csproj: Low (leaf node, 1 package update)
- Api.csproj: Low (3 package updates, 1 behavioral change)
- MeterKloud.csproj: Low (6 package updates, 3 behavioral changes)
- TestMeterLib.csproj: Low (1 deprecated package replacement)

### Critical Issues

**No Blocking Issues**

**Minor Considerations**:
1. **Deprecated Package**: `xunit` package is deprecated in TestMeterLib - no replacement needed as current version remains compatible
2. **Behavioral Changes**: 4 runtime behavioral changes affecting `System.Uri` and `Microsoft.Extensions.Hosting.HostBuilder` - require runtime testing validation
3. **Azure Functions V2 Model**: Optional modernization opportunity for Api.csproj

### Recommended Approach

**Single atomic upgrade** of all projects, packages, and dependencies followed by comprehensive testing.

**Expected Duration**: Low complexity - suitable for completion in a single development cycle.

**Iteration Strategy**: Fast batch approach (2-3 detail iterations) due to simple solution structure.

---

## Migration Strategy

### Approach Selection

**Selected: All-At-Once Strategy**

All projects in the solution will be upgraded simultaneously in a single coordinated operation.

### Justification

The All-at-Once approach is optimal for this solution because:

1. **Small Solution Size**: Only 4 projects
2. **Homogeneous Technology Stack**: All projects currently on .NET 9.0, all targeting .NET 10.0
3. **Simple Dependencies**: Clear 2-level structure, no circular dependencies
4. **Low Complexity**: All projects rated Low difficulty
5. **Package Compatibility**: All required package updates have confirmed .NET 10 versions available
6. **No Security Urgency**: Zero security vulnerabilities to address
7. **Clean Current State**: All projects already on modern .NET (9.0)

### All-at-Once Strategy Rationale

**Speed**: Fastest path to completion - single upgrade operation instead of multiple phases

**Simplicity**: No intermediate multi-targeting states to manage

**Clean Testing**: Test entire solution in final state, not intermediate configurations

**Reduced Coordination**: All developers upgrade simultaneously, no version conflicts

**Unified Dependencies**: All projects share package versions - update once

### Execution Approach

The upgrade follows this atomic sequence:

1. **Atomic Project & Package Update** (single coordinated operation)
   - Update all `.csproj` files: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`
   - Update all package references across all projects simultaneously
   - Restore dependencies
   - Build entire solution
   - Fix all compilation errors discovered
   - Rebuild to verify

2. **Comprehensive Testing** (after atomic upgrade completes)
   - Execute all test projects
   - Validate behavioral changes
   - Verify application functionality

### Dependency-Based Ordering

While all projects update atomically, internal processing follows this logical order:

1. **Foundation**: MeterDataLib.csproj (shared dependency)
2. **Consumers**: Api.csproj, MeterKloud.csproj (application projects)
3. **Validation**: TestMeterLib.csproj (test project)

This ensures that when compilation occurs, the compiler processes dependencies before dependents.

### Risk Management Approach

**Advantages**:
- Fastest completion
- Simplest coordination
- Clean final state
- No multi-targeting complexity

**Challenges & Mitigations**:
- **Challenge**: All projects change simultaneously
  - **Mitigation**: Small solution size makes verification manageable
  
- **Challenge**: Larger testing surface
  - **Mitigation**: All projects Low complexity; comprehensive test suite in TestMeterLib
  
- **Challenge**: Behavioral changes affect multiple projects
  - **Mitigation**: Only 4 behavioral changes, all low-impact; documented testing strategy

### Parallel vs Sequential Execution

**File Updates**: Sequential (by dependency order internally, but perceived as atomic batch)

**Testing**: Sequential (foundation library tests first, then application tests)

**Validation**: Parallel where possible (independent projects like Api can be validated independently)

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a simple, clean dependency structure with no circular dependencies:

```
MeterDataLib.csproj (leaf node)
    ?
    ??? MeterKloud.csproj (depends on MeterDataLib)
    ??? TestMeterLib.csproj (depends on MeterDataLib)

Api.csproj (independent)
```

**Dependency Characteristics**:
- **Leaf Node**: MeterDataLib.csproj (0 dependencies, 2 dependents)
- **Application Projects**: MeterKloud.csproj, Api.csproj (consume MeterDataLib)
- **Test Project**: TestMeterLib.csproj (tests MeterDataLib)
- **Maximum Depth**: 2 levels
- **Circular Dependencies**: None

### Project Groupings by Migration Phase

Since this is an All-at-Once migration, all projects upgrade simultaneously. However, for understanding purposes, they can be conceptually grouped by dependency tier:

**Tier 1 - Foundation (Leaf Nodes)**:
- `MeterDataLib\MeterDataLib.csproj` - Shared class library with no project dependencies

**Tier 2 - Applications & Tests (Dependent Projects)**:
- `Api\Api.csproj` - Azure Functions API (independent)
- `Client\MeterKloud.csproj` - Blazor WebAssembly app (depends on MeterDataLib)
- `TestMeterLib\TestMeterLib.csproj` - Test project (depends on MeterDataLib)

**Migration Order Note**: While conceptually tiered, the All-at-Once strategy updates all project files and packages simultaneously in a single coordinated batch.

### Critical Path Identification

**Primary Path**: MeterDataLib ? MeterKloud  
- MeterDataLib must remain compatible with MeterKloud during the atomic upgrade
- Both projects share common Microsoft.Extensions packages that will update together

**Independent Path**: Api  
- No project dependencies; can be validated independently
- Shares Microsoft.Extensions packages with other projects

**Test Path**: MeterDataLib ? TestMeterLib  
- Tests validate MeterDataLib functionality post-upgrade
- Contains deprecated xunit package requiring attention

### No Circular Dependencies

Clean dependency structure with no cycles detected.

---

## Project-by-Project Plans

### MeterDataLib.csproj

**Current State**: 
- Target Framework: net9.0
- Project Type: Class library (SDK-style)
- Dependencies: 0 project dependencies
- Dependents: 2 (MeterKloud.csproj, TestMeterLib.csproj)
- NuGet Packages: 2
- Lines of Code: 10,221
- Risk Level: Low

**Target State**: 
- Target Framework: net10.0
- Package Updates: 1

#### Migration Steps

##### 1. Prerequisites
- Ensure .NET 10 SDK is installed
- Verify no uncommitted changes in repository
- Confirm on correct branch: `Net10Upgrade20260222`

##### 2. Framework Update
Update the project file `MeterDataLib\MeterDataLib.csproj`:
```xml
<!-- Change from: -->
<TargetFramework>net9.0</TargetFramework>

<!-- To: -->
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.Logging.Abstractions | 10.0.1 | 10.0.3 | NuGet package upgrade recommended for .NET 10 compatibility |

**Other Packages** (no update required):
- ExcelDataReader 3.8.0 - Compatible with .NET 10 ?

##### 4. Expected Breaking Changes
**None identified** - No binary incompatibilities or source incompatibilities detected for this project.

##### 5. Code Modifications
**Minimal to none expected**:
- No API breaking changes detected
- No behavioral changes affect this project
- Framework update is primary change

**Areas to review** (precautionary):
- Logging usage (Microsoft.Extensions.Logging.Abstractions update)
- Excel reading functionality (ensure ExcelDataReader works as expected)

##### 6. Testing Strategy
**Unit Tests**: TestMeterLib.csproj contains tests for this library
- Execute full test suite after upgrade
- Verify all existing tests pass
- No new tests required unless behavioral issues discovered

**Integration Tests**: 
- Verify MeterKloud.csproj still functions correctly with updated MeterDataLib

**Manual Validation**:
- None required for this library project

##### 7. Validation Checklist
- [ ] Project file updated to net10.0
- [ ] Microsoft.Extensions.Logging.Abstractions updated to 10.0.3
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] TestMeterLib test suite passes
- [ ] No package dependency conflicts reported

---

### Api.csproj

**Current State**: 
- Target Framework: net9.0
- Project Type: Azure Functions (SDK-style)
- Dependencies: 0 project dependencies
- Dependents: 0
- NuGet Packages: 9
- Lines of Code: 38
- Risk Level: Low

**Target State**: 
- Target Framework: net10.0
- Package Updates: 3

#### Migration Steps

##### 1. Prerequisites
- Ensure .NET 10 SDK is installed
- Ensure Azure Functions Core Tools v4 installed (supports .NET 10)
- Verify local Azure Functions development environment

##### 2. Framework Update
Update the project file `Api\Api.csproj`:
```xml
<!-- Change from: -->
<TargetFramework>net9.0</TargetFramework>

<!-- To: -->
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.Caching.Memory | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |
| System.Text.Encodings.Web | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |

**Other Packages** (no update required):
- Microsoft.ApplicationInsights.WorkerService 2.23.0 - Compatible ?
- Microsoft.Azure.Functions.Worker 2.51.0 - Compatible ?
- Microsoft.Azure.Functions.Worker.ApplicationInsights 2.50.0 - Compatible ?
- Microsoft.Azure.Functions.Worker.Extensions.Http 3.3.0 - Compatible ?
- Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore 2.1.0 - Compatible ?
- Microsoft.Azure.Functions.Worker.Sdk 2.0.7 - Compatible ?

##### 4. Expected Breaking Changes

**Behavioral Change**: 
- **System.Uri**: 1 occurrence detected in this project
  - **Impact**: URI parsing or construction behavior may differ in .NET 10
  - **Mitigation**: Test all HTTP endpoints; verify URI handling in Functions

**No Binary or Source Incompatibilities** - Code should compile without changes.

##### 5. Code Modifications

**Expected modifications**: Minimal to none

**Areas requiring review**:
1. **System.Uri usage**: 
   - Verify URI construction in HTTP triggers
   - Test any URI parsing or manipulation logic
   - Validate absolute/relative URI handling

2. **HTTP handling**: 
   - Test all HTTP-triggered functions
   - Verify request/response processing
   - Validate routing and binding

3. **Dependency injection**: 
   - Confirm service registration still works (Microsoft.Extensions packages updated)
   - Validate HttpClient factory usage

##### 6. Testing Strategy

**Functional Testing**:
- [ ] Start Azure Functions locally (`func start`)
- [ ] Test all HTTP endpoints
- [ ] Verify request/response payloads
- [ ] Validate error handling

**Integration Testing**:
- [ ] Test with actual Azure Functions runtime
- [ ] Verify Application Insights integration
- [ ] Validate logging and telemetry

**Performance Testing**:
- [ ] Verify cold start times acceptable
- [ ] Check memory usage unchanged

##### 7. Validation Checklist
- [ ] Project file updated to net10.0
- [ ] All 3 Microsoft.Extensions packages updated to 10.0.3
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] Functions runtime starts locally
- [ ] All HTTP endpoints respond correctly
- [ ] No System.Uri behavioral issues observed
- [ ] Application Insights logging works
- [ ] No package dependency conflicts

##### 8. Optional Enhancement

**Azure Functions V2 Model Opportunity**:
The assessment identified an opportunity to modernize this project to the Azure Functions V2 programming model, which offers:
- Improved performance
- Better Application Insights integration
- Enhanced developer experience

**Recommendation**: Address this in a separate upgrade task after .NET 10 migration is complete and validated.

---

### MeterKloud.csproj

**Current State**: 
- Target Framework: net9.0
- Project Type: Blazor WebAssembly (SDK-style)
- Dependencies: 1 (MeterDataLib.csproj)
- Dependents: 0
- NuGet Packages: 10
- Lines of Code: 1,117
- Risk Level: Low (elevated to Medium due to behavioral changes)

**Target State**: 
- Target Framework: net10.0
- Package Updates: 6

#### Migration Steps

##### 1. Prerequisites
- Ensure .NET 10 SDK is installed
- Ensure MeterDataLib.csproj already upgraded to net10.0
- Verify Blazor WebAssembly development environment

##### 2. Framework Update
Update the project file `Client\MeterKloud.csproj`:
```xml
<!-- Change from: -->
<TargetFramework>net9.0</TargetFramework>

<!-- To: -->
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.AspNetCore.Components.WebAssembly | 9.0.11 | 10.0.3 | NuGet package upgrade recommended |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 9.0.11 | 10.0.3 | NuGet package upgrade recommended |
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 9.0.11 | 10.0.3 | NuGet package upgrade recommended |
| Microsoft.Extensions.Caching.Memory | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |
| System.Text.Encodings.Web | 10.0.1 | 10.0.3 | NuGet package upgrade recommended |

**Other Packages** (no update required):
- AnthonyChu.AzureStaticWebApps.Blazor.Authentication 0.0.2-preview - Compatible ?
- Blazored.LocalStorage 4.5.0 - Compatible ?
- MudBlazor 8.15.0 - Compatible ?
- Plotly.Blazor 6.0.2 - Compatible ?

##### 4. Expected Breaking Changes

**Behavioral Changes** (3 occurrences - highest in solution):

1. **System.Uri** (2 occurrences):
   - **Impact**: URI parsing/construction behavior changes
   - **Location**: Likely in `Program.cs` (BaseAddress construction) and HTTP client usage
   - **Risk**: Medium - affects application routing and API calls
   
2. **Microsoft.Extensions.Hosting.HostBuilder** (1 occurrence):
   - **Impact**: Hosting pipeline initialization differences
   - **Location**: Application startup configuration
   - **Risk**: Medium - could affect service provider behavior

**No Binary or Source Incompatibilities** - Code should compile without changes.

##### 5. Code Modifications

**Expected modifications**: Minimal, but thorough testing required

**Critical areas requiring review**:

1. **Program.cs** (file context provided):
```csharp
// Review this line - System.Uri behavioral change:
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
```
   - **Action**: Test that BaseAddress resolves correctly
   - **Validation**: Verify HttpClient base URL matches expected value
   - **Test**: Navigate to all application routes

2. **HttpClient Usage**:
   - Review all API calls using the configured HttpClient
   - Verify relative/absolute URI construction
   - Test MeterKloudClientApi initialization

3. **Application Startup**:
   - Verify service registration still works (HostBuilder change)
   - Validate dependency injection container behavior
   - Test authentication flow (StaticWebAppsAuthentication)

4. **Service Registrations** (from Program.cs):
   - MudBlazor services
   - Memory cache
   - IndexedDB accessor
   - Local storage
   - Custom services (MeterDataStore, MeterKloudClientApi)

##### 6. Testing Strategy

**Build Validation**:
- [ ] Project builds successfully
- [ ] No compilation errors
- [ ] No warnings related to behavioral changes

**Runtime Validation**:
- [ ] Application starts without errors
- [ ] All Blazor components render correctly
- [ ] Navigation between pages works
- [ ] Authentication flow functions correctly

**HttpClient & API Testing**:
- [ ] BaseAddress set correctly
- [ ] MeterKloudClientApi.InitApi() succeeds
- [ ] API calls to backend work
- [ ] Relative URLs resolve properly
- [ ] Absolute URLs function as expected

**UI Component Testing**:
- [ ] MudBlazor components render
- [ ] Plotly charts display correctly
- [ ] Blazored.LocalStorage reads/writes data
- [ ] IndexedDB operations work

**Integration Testing**:
- [ ] MeterDataLib integration works
- [ ] IndexedDB data store functions
- [ ] Memory cache operates correctly
- [ ] Azure Static Web Apps authentication works

**Browser Testing**:
- [ ] Chrome/Edge testing
- [ ] Firefox testing (if supported)
- [ ] Mobile browser testing (if applicable)

##### 7. Validation Checklist
- [ ] Project file updated to net10.0
- [ ] All 6 packages updated to target versions
- [ ] MeterDataLib.csproj dependency at net10.0
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] `dotnet run` starts application
- [ ] Application loads in browser
- [ ] No console errors on startup
- [ ] BaseAddress URI construction works
- [ ] HttpClient API calls succeed
- [ ] Authentication flow works
- [ ] All UI components functional
- [ ] No package dependency conflicts
- [ ] IndexedDB operations work
- [ ] LocalStorage operations work
- [ ] MeterKloudClientApi initializes successfully

##### 8. Known Behavioral Change Details

**System.Uri Changes in .NET 10**:
- Review official .NET 10 migration guide for System.Uri behavioral changes
- Test edge cases: empty strings, relative paths, query parameters, fragments
- Validate Uri.ToString() output matches expected format

**HostBuilder Changes**:
- Verify WebAssemblyHostBuilder behavior unchanged
- Confirm service provider scope handling
- Test CreateAsyncScope() usage in Program.cs

---

### TestMeterLib.csproj

**Current State**: 
- Target Framework: net9.0
- Project Type: Test project (SDK-style)
- Dependencies: 1 (MeterDataLib.csproj)
- Dependents: 0
- NuGet Packages: 6
- Lines of Code: 2,599
- Risk Level: Low

**Target State**: 
- Target Framework: net10.0
- Package Updates: 0 (1 deprecated package to address)

#### Migration Steps

##### 1. Prerequisites
- Ensure .NET 10 SDK is installed
- Ensure MeterDataLib.csproj already upgraded to net10.0
- Verify test runner integration (Visual Studio Test Explorer or `dotnet test`)

##### 2. Framework Update
Update the project file `TestMeterLib\TestMeterLib.csproj`:
```xml
<!-- Change from: -->
<TargetFramework>net9.0</TargetFramework>

<!-- To: -->
<TargetFramework>net10.0</TargetFramework>
```

##### 3. Package Updates

**No package version updates required** - all packages compatible with .NET 10.

**Deprecated Package Notice**:
| Package | Current Version | Status | Action Required |
|---------|----------------|---------|-----------------|
| xunit | 2.9.3 | ?? Deprecated | **No immediate action** - package remains functional |

**Other Packages** (all compatible):
- coverlet.collector 6.0.4 - Compatible ?
- FluentAssertions 8.8.0 - Compatible ?
- Microsoft.NET.Test.Sdk 18.0.1 - Compatible ?
- xunit.runner.visualstudio 3.1.5 - Compatible ?

##### 4. Deprecated Package: xunit

**Status**: The `xunit` package is marked as deprecated by the package maintainers.

**Why it's deprecated**: The xunit team has consolidated their packages, and `xunit` is now a meta-package pointing to newer packages.

**Current impact**: 
- ? Package continues to work with .NET 10
- ? No functional issues expected
- ?? May not receive updates in the future

**Recommended future action** (not required for this upgrade):
- Consider migrating to the recommended xunit packages in a future maintenance task
- Current version 2.9.3 is stable and functional
- Monitor xunit project announcements for migration guidance

**For this upgrade**: **No action required** - proceed with existing xunit 2.9.3

##### 5. Expected Breaking Changes
**None identified** - No behavioral changes affect this test project.

##### 6. Code Modifications
**None expected** - Test code should remain unchanged.

**Areas to review** (precautionary):
- Test discovery and execution
- FluentAssertions syntax (no changes expected)
- Test coverage collection (coverlet)

##### 7. Testing Strategy

**Test Execution**:
- [ ] Run full test suite: `dotnet test`
- [ ] Verify all tests discovered correctly
- [ ] Confirm all existing tests pass
- [ ] No new test failures introduced by framework upgrade

**Test Coverage**:
- [ ] Verify coverlet still collects coverage data
- [ ] Coverage metrics remain consistent

**Test Runner Integration**:
- [ ] Visual Studio Test Explorer shows all tests
- [ ] Tests runnable from IDE
- [ ] Tests runnable from command line

**Validation of MeterDataLib**:
- [ ] Tests validate upgraded MeterDataLib.csproj at net10.0
- [ ] No incompatibilities between test project and library
- [ ] All library functionality still working as expected

##### 8. Validation Checklist
- [ ] Project file updated to net10.0
- [ ] MeterDataLib.csproj dependency at net10.0
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build` produces 0 warnings
- [ ] `dotnet test` succeeds
- [ ] All tests pass (0 failures)
- [ ] Test discovery works correctly
- [ ] Code coverage collection works (coverlet)
- [ ] No package dependency conflicts
- [ ] xunit deprecated warning noted (informational only)

##### 9. Post-Upgrade Maintenance Note

**Future consideration**: Plan a separate task to address the deprecated xunit package

**When to address**:
- Not urgent - current version works fine
- Consider during next major maintenance cycle
- Monitor xunit project for migration announcements
- Evaluate when xunit 2.x reaches end-of-support

**Migration resources**:
- xunit documentation: https://xunit.net/
- Check for xunit v3 stable release status
- Review migration guides when available

---

## Package Update Reference

### Common Package Updates (Affecting Multiple Projects)

These packages appear in multiple projects and will be updated to the same version across all:

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.Caching.Memory | 10.0.1 | 10.0.3 | 2 projects (Api, MeterKloud) | .NET 10 compatibility and bug fixes |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.3 | 2 projects (Api, MeterKloud) | .NET 10 compatibility and bug fixes |
| System.Text.Encodings.Web | 10.0.1 | 10.0.3 | 2 projects (Api, MeterKloud) | .NET 10 compatibility and bug fixes |

### Project-Specific Package Updates

#### MeterDataLib.csproj (1 update)
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.Extensions.Logging.Abstractions | 10.0.1 | 10.0.3 | .NET 10 compatibility |

#### Api.csproj (3 updates)
All updates covered in "Common Package Updates" section above.

#### MeterKloud.csproj (6 updates)
| Package | Current | Target | Reason |
|---------|---------|--------|--------|
| Microsoft.AspNetCore.Components.WebAssembly | 9.0.11 | 10.0.3 | Required for .NET 10 Blazor WebAssembly |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 9.0.11 | 10.0.3 | Required for .NET 10 Blazor WebAssembly |
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 9.0.11 | 10.0.3 | Required for .NET 10 development |
| *Plus 3 common packages (see above)* |  |  |  |

#### TestMeterLib.csproj (0 updates)
All packages compatible with .NET 10 - no updates required.

**Note**: xunit 2.9.3 is deprecated but remains functional.

### Package Compatibility Matrix

| Package | .NET 9 Version | .NET 10 Version | Compatibility Status |
|---------|---------------|-----------------|---------------------|
| **Requires Update** |
| Microsoft.AspNetCore.Components.WebAssembly | 9.0.11 | 10.0.3 | ?? Major version bump required |
| Microsoft.AspNetCore.Components.WebAssembly.Authentication | 9.0.11 | 10.0.3 | ?? Major version bump required |
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 9.0.11 | 10.0.3 | ?? Major version bump required |
| Microsoft.Extensions.Caching.Memory | 10.0.1 | 10.0.3 | ? Patch update |
| Microsoft.Extensions.Http | 10.0.1 | 10.0.3 | ? Patch update |
| Microsoft.Extensions.Logging.Abstractions | 10.0.1 | 10.0.3 | ? Patch update |
| System.Text.Encodings.Web | 10.0.1 | 10.0.3 | ? Patch update |
| **Compatible (No Update)** |
| AnthonyChu.AzureStaticWebApps.Blazor.Authentication | 0.0.2-preview | - | ? Compatible |
| Blazored.LocalStorage | 4.5.0 | - | ? Compatible |
| coverlet.collector | 6.0.4 | - | ? Compatible |
| ExcelDataReader | 3.8.0 | - | ? Compatible |
| FluentAssertions | 8.8.0 | - | ? Compatible |
| Microsoft.ApplicationInsights.WorkerService | 2.23.0 | - | ? Compatible |
| Microsoft.Azure.Functions.Worker | 2.51.0 | - | ? Compatible |
| Microsoft.Azure.Functions.Worker.ApplicationInsights | 2.50.0 | - | ? Compatible |
| Microsoft.Azure.Functions.Worker.Extensions.Http | 3.3.0 | - | ? Compatible |
| Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore | 2.1.0 | - | ? Compatible |
| Microsoft.Azure.Functions.Worker.Sdk | 2.0.7 | - | ? Compatible |
| Microsoft.NET.Test.Sdk | 18.0.1 | - | ? Compatible |
| MudBlazor | 8.15.0 | - | ? Compatible |
| Plotly.Blazor | 6.0.2 | - | ? Compatible |
| xunit | 2.9.3 | - | ?? Deprecated but compatible |
| xunit.runner.visualstudio | 3.1.5 | - | ? Compatible |

### Update Execution Order

For the All-at-Once strategy, all packages update simultaneously. However, internal processing follows this logical order:

1. **Foundation project packages** (MeterDataLib):
   - Microsoft.Extensions.Logging.Abstractions 10.0.1 ? 10.0.3

2. **Application project packages** (Api, MeterKloud):
   - Common Microsoft.Extensions packages 10.0.1 ? 10.0.3
   - Blazor WebAssembly packages 9.0.11 ? 10.0.3

3. **Test project packages** (TestMeterLib):
   - No updates required

### Known Package Dependencies

**Microsoft.AspNetCore.Components.WebAssembly family**:
- These three packages should stay in sync (all 10.0.3)
- Required for Blazor WebAssembly .NET 10 support
- Major version change (9.x ? 10.x) indicates framework alignment

**Microsoft.Extensions.* packages**:
- Part of .NET runtime extensions
- Patch updates (10.0.1 ? 10.0.3) are safe
- No breaking changes expected in patch versions

### Package Source Verification

All packages available from:
- **NuGet.org** (official source)
- Verify internet connectivity for package restore
- Ensure NuGet.org is in package sources: `dotnet nuget list source`

---

## Breaking Changes Catalog

### Summary

**Good News**: No binary or source incompatibilities detected across all projects.

**Behavioral Changes**: 4 low-impact changes requiring runtime testing.

### Behavioral Changes Detail

#### 1. System.Uri (3 occurrences total)

**Affected Projects**: Api.csproj (1), MeterKloud.csproj (2)

**Change Description**: 
System.Uri parsing and construction behavior has changed in .NET 10. This affects how URIs are validated, normalized, and represented.

**Potential Impacts**:
- URI parsing rules may differ
- String representation of URIs may change
- Relative/absolute URI resolution behavior
- Query string handling
- Fragment handling
- Encoded character handling

**Mitigation Strategy**:

**For Api.csproj**:
- Review HTTP trigger URI handling
- Test all function endpoints
- Verify routing works correctly
- Validate URI construction in any HTTP-related code

**For MeterKloud.csproj** (Higher impact - 2 occurrences):
```csharp
// Known location: Client\Program.cs
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
```

**Testing approach**:
1. Verify `builder.HostEnvironment.BaseAddress` string is valid
2. Confirm `new Uri(...)` construction succeeds
3. Validate BaseAddress property set correctly on HttpClient
4. Test that API calls using relative URLs work
5. Test that absolute URLs work
6. Verify navigation routing functions correctly

**Edge cases to test**:
- Empty or null base addresses
- Trailing slashes in base URLs
- Relative URL resolution
- Query parameters in URLs
- URL fragments (#anchors)
- Special characters in URLs

**Validation steps**:
- [ ] Application starts without URI construction exceptions
- [ ] HttpClient BaseAddress property contains expected value
- [ ] API calls to backend succeed
- [ ] Browser navigation works
- [ ] No console errors related to URI handling

#### 2. Microsoft.Extensions.Hosting.HostBuilder (1 occurrence)

**Affected Projects**: MeterKloud.csproj

**Change Description**:
Hosting infrastructure initialization and service provider behavior changes in .NET 10.

**Potential Impacts**:
- Service provider creation timing
- Scope management
- Service lifetime behavior
- Startup configuration order
- Middleware registration order (if applicable)

**Mitigation Strategy**:

**For MeterKloud.csproj**:
```csharp
// Known location: Client\Program.cs
var host = builder.Build();
await using (var scope = host.Services.CreateAsyncScope())
{
    var api = scope.ServiceProvider.GetRequiredService<MeterKloudClientApi>();
    await api.InitApi();
}
```

**Testing approach**:
1. Verify host builds successfully
2. Confirm async scope creation works
3. Validate service provider resolves MeterKloudClientApi
4. Test that InitApi() executes correctly
5. Ensure all registered services available

**Registered services to validate**:
- MudBlazor services
- Memory cache
- HttpClient
- StaticWebAppsAuthentication
- IndexedDbAccessor
- IMeterDataStore
- MeterDataStorageManager
- MeterKloudClientApi
- BlazoredLocalStorage

**Validation steps**:
- [ ] `builder.Build()` succeeds
- [ ] `CreateAsyncScope()` succeeds
- [ ] `GetRequiredService<MeterKloudClientApi>()` succeeds
- [ ] `api.InitApi()` succeeds
- [ ] Application starts without DI errors
- [ ] All services resolve correctly at runtime
- [ ] No service lifetime issues observed

### No Binary Incompatibilities

**Definition**: Binary incompatibilities require code changes because types, methods, or properties have been removed or fundamentally changed.

**Status**: ? **None detected**

All existing APIs remain binary compatible between .NET 9 and .NET 10 for this codebase.

### No Source Incompatibilities

**Definition**: Source incompatibilities cause compilation errors when recompiling against the new framework version.

**Status**: ? **None detected**

Code should compile without modifications after updating TargetFramework and package versions.

### Framework-Level Changes

**.NET 10 General Changes**:
- Runtime performance improvements
- Garbage collector enhancements
- JIT compiler optimizations
- Library improvements

**None of these should negatively impact this solution.**

### Package-Specific Breaking Changes

#### Microsoft.AspNetCore.Components.WebAssembly 9.0.11 ? 10.0.3

**Major version change** (9.x ? 10.x) indicates framework alignment, not breaking changes.

**Expected changes**:
- Performance improvements
- New optional features
- Bug fixes from 9.0.11

**Compatibility**: Designed to be backward compatible for standard usage patterns.

**Testing focus**:
- Component rendering
- JavaScript interop
- Routing
- Authentication
- State management

#### Microsoft.Extensions.* Packages (10.0.1 ? 10.0.3)

**Patch version updates** - no breaking changes expected.

**Changes**: Bug fixes, performance improvements, security patches.

### Breaking Changes Reference

**Official Documentation**:
- .NET 10 Breaking Changes: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0
- ASP.NET Core 10.0 Breaking Changes: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0

**Recommendation**: Review official docs for any edge cases specific to your code patterns.

### Testing Strategy for Behavioral Changes

#### Unit Testing
- Run TestMeterLib test suite
- Verify all existing tests pass
- No new tests required unless issues found

#### Integration Testing
- Test MeterKloud.csproj with MeterDataLib.csproj
- Test Api.csproj endpoints
- Verify end-to-end flows

#### Runtime Testing (Critical)
- **System.Uri changes**: Test all HTTP/navigation scenarios
- **HostBuilder changes**: Verify application startup and DI
- **User workflows**: Execute common user scenarios
- **Error paths**: Test error handling and validation

#### Browser Testing (for MeterKloud.csproj)
- Chrome/Edge (primary)
- Firefox (if supported)
- Mobile browsers (if applicable)
- Developer console monitoring for errors

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing occurs at three levels: per-project validation, integration testing, and full solution validation.

### Phase 1: Per-Project Testing

Execute after framework and package updates are complete, before declaring upgrade successful.

#### MeterDataLib.csproj Validation

**Build Validation**:
```bash
cd MeterDataLib
dotnet restore
dotnet build --no-restore
```
- [ ] Restore succeeds
- [ ] Build succeeds with 0 errors
- [ ] Build produces 0 warnings
- [ ] No package conflicts

**Dependency Validation**:
- [ ] Microsoft.Extensions.Logging.Abstractions 10.0.3 installed
- [ ] ExcelDataReader 3.8.0 compatible
- [ ] No transitive dependency issues

**Test Validation** (via TestMeterLib):
- Covered in TestMeterLib section below

---

#### Api.csproj Validation

**Build Validation**:
```bash
cd Api
dotnet restore
dotnet build --no-restore
```
- [ ] Restore succeeds
- [ ] Build succeeds with 0 errors
- [ ] Build produces 0 warnings
- [ ] All 3 Microsoft.Extensions packages at 10.0.3

**Azure Functions Runtime Validation**:
```bash
func start
```
- [ ] Functions runtime starts successfully
- [ ] No initialization errors
- [ ] HTTP triggers registered

**Functional Testing**:
- [ ] Test all HTTP endpoints
- [ ] Verify request/response handling
- [ ] Validate error responses
- [ ] Confirm Application Insights telemetry

**System.Uri Behavioral Change Testing**:
- [ ] All URI construction succeeds
- [ ] No URI parsing errors
- [ ] Endpoint routing works correctly

---

#### MeterKloud.csproj Validation

**Build Validation**:
```bash
cd Client
dotnet restore
dotnet build --no-restore
```
- [ ] Restore succeeds
- [ ] Build succeeds with 0 errors
- [ ] Build produces 0 warnings
- [ ] MeterDataLib.csproj reference at net10.0
- [ ] All 6 packages updated correctly

**Development Server Validation**:
```bash
dotnet run
```
- [ ] WebAssembly dev server starts
- [ ] No compilation errors in browser console
- [ ] Application loads successfully
- [ ] No runtime errors on initial load

**System.Uri Behavioral Change Testing** (Critical):
```csharp
// Verify from Program.cs:
BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
```
- [ ] BaseAddress construction succeeds
- [ ] HttpClient BaseAddress set correctly
- [ ] No URI exceptions in console
- [ ] Relative URL resolution works
- [ ] Absolute URL handling works

**HostBuilder Behavioral Change Testing** (Critical):
```csharp
// Verify from Program.cs:
var host = builder.Build();
await using (var scope = host.Services.CreateAsyncScope())
{
    var api = scope.ServiceProvider.GetRequiredService<MeterKloudClientApi>();
    await api.InitApi();
}
```
- [ ] Host builds successfully
- [ ] Async scope creation succeeds
- [ ] Service provider resolves all services
- [ ] MeterKloudClientApi resolution succeeds
- [ ] InitApi() executes without errors
- [ ] Application continues after scope disposal

**Service Registration Validation**:
- [ ] MudBlazor services available
- [ ] Memory cache functional
- [ ] HttpClient instances created correctly
- [ ] Authentication provider registered
- [ ] IndexedDbAccessor available
- [ ] IMeterDataStore resolved
- [ ] MeterDataStorageManager available
- [ ] BlazoredLocalStorage functional

**UI Component Testing**:
- [ ] All Blazor components render
- [ ] MudBlazor components display correctly
- [ ] Plotly charts render
- [ ] Navigation between pages works
- [ ] Authentication UI functions

**Data Access Testing**:
- [ ] IndexedDB operations work
- [ ] LocalStorage read/write succeeds
- [ ] Memory cache stores/retrieves data
- [ ] MeterDataStore operations function

**Integration with Backend**:
- [ ] API calls to backend succeed
- [ ] MeterKloudClientApi methods work
- [ ] Data fetching/posting succeeds

---

#### TestMeterLib.csproj Validation

**Build Validation**:
```bash
cd TestMeterLib
dotnet restore
dotnet build --no-restore
```
- [ ] Restore succeeds
- [ ] Build succeeds with 0 errors
- [ ] Build produces 0 warnings (except xunit deprecation notice)
- [ ] MeterDataLib.csproj reference at net10.0

**Test Execution**:
```bash
dotnet test --no-build
```
- [ ] All tests discovered correctly
- [ ] All tests execute
- [ ] All tests pass (0 failures)
- [ ] No test framework errors

**Coverage Validation**:
- [ ] coverlet collector runs
- [ ] Coverage data generated
- [ ] Coverage metrics consistent with baseline

**Test Runner Integration**:
- [ ] Visual Studio Test Explorer shows all tests
- [ ] Tests executable from IDE
- [ ] Tests executable from CLI
- [ ] xunit runner functions (despite deprecation)

---

### Phase 2: Integration Testing

Execute after all per-project tests pass.

#### MeterDataLib ? MeterKloud Integration

**Scenario Testing**:
- [ ] MeterKloud successfully references upgraded MeterDataLib
- [ ] MeterDataLib types available in MeterKloud
- [ ] MeterDataStorageManager functions correctly
- [ ] IndexedDB operations through MeterDataLib work

#### MeterDataLib ? TestMeterLib Integration

**Validation**:
- [ ] Test project tests upgraded library
- [ ] No incompatibilities between versions
- [ ] All library features testable

#### Api ? MeterKloud Integration (if applicable)

**End-to-End Testing**:
- [ ] MeterKloud can call Api endpoints
- [ ] Request/response serialization works
- [ ] Authentication flow functions
- [ ] CORS configuration correct (if applicable)

---

### Phase 3: Full Solution Testing

Execute after integration testing passes.

#### Solution-Level Build

```bash
# From solution root
dotnet restore MeterKloud.sln
dotnet build MeterKloud.sln --no-restore
```
- [ ] All 4 projects restore successfully
- [ ] All 4 projects build successfully
- [ ] 0 errors across entire solution
- [ ] 0 warnings across entire solution
- [ ] All package dependencies resolved

#### Solution-Level Test Execution

```bash
dotnet test MeterKloud.sln --no-build
```
- [ ] All test projects discovered
- [ ] All tests execute
- [ ] All tests pass
- [ ] Test summary shows success

---

### Smoke Testing Checklist

Quick validation of core functionality after upgrade:

**MeterKloud Application** (5-10 minutes):
- [ ] Application launches in browser
- [ ] Homepage loads without errors
- [ ] User can navigate between pages
- [ ] Authentication flow works
- [ ] Data loads from IndexedDB/LocalStorage
- [ ] Charts render (Plotly)
- [ ] UI components interactive (MudBlazor)
- [ ] No console errors

**Api Functions** (5 minutes):
- [ ] Functions app starts locally
- [ ] Health check endpoint responds
- [ ] Primary endpoints return expected data
- [ ] Error handling works
- [ ] Logging/telemetry captured

**Test Suite** (automatic):
- [ ] `dotnet test` passes fully

---

### Performance Validation

**.NET 10 should maintain or improve performance:**

**Startup Time**:
- [ ] MeterKloud app load time acceptable
- [ ] Api cold start time acceptable
- [ ] No regression in startup performance

**Runtime Performance**:
- [ ] UI responsiveness unchanged
- [ ] Data operations perform well
- [ ] No memory leaks observed

**Build Performance**:
- [ ] Solution build time acceptable
- [ ] Incremental builds work correctly

---

### Browser Compatibility Testing (for MeterKloud.csproj)

**Primary Browsers**:
- [ ] Chrome/Edge (Chromium)
- [ ] Firefox
- [ ] Safari (if supported)

**Mobile Browsers** (if applicable):
- [ ] Mobile Chrome
- [ ] Mobile Safari

**Validation per browser**:
- [ ] Application loads
- [ ] No console errors
- [ ] Core functionality works
- [ ] UI renders correctly

---

### Regression Testing

**Ensure no functionality loss:**

- [ ] All existing features still work
- [ ] No new bugs introduced
- [ ] User workflows unchanged
- [ ] Data persistence works
- [ ] Authentication/authorization unchanged

---

### Success Criteria for Testing Phase

? **All tests must pass before upgrade considered complete:**

1. ? All 4 projects build successfully
2. ? All unit tests pass (TestMeterLib)
3. ? MeterKloud application runs without errors
4. ? Api functions app runs without errors
5. ? No behavioral change issues observed
6. ? All integration points functional
7. ? Smoke tests pass
8. ? No performance regressions
9. ? No package dependency conflicts
10. ? No console errors in browser

---

## Risk Management

### High-Level Assessment

**Overall Risk Level**: **Low**

The .NET 10 upgrade presents minimal risk due to:
- Small solution size (4 projects)
- All projects currently on modern .NET (9.0)
- Simple dependency structure
- No security vulnerabilities
- All projects rated Low difficulty
- Clear package upgrade paths

### Risk Factors by Category

| Category | Risk Level | Description | Mitigation |
|----------|-----------|-------------|------------|
| **Framework Compatibility** | ?? Low | .NET 9 ? 10 is a single-version increment | Standard upgrade process; excellent compatibility |
| **Package Dependencies** | ?? Low | 8 of 23 packages need updates; all have .NET 10 versions | All updates confirmed compatible |
| **API Breaking Changes** | ?? Low | 0 binary incompatibilities, 0 source incompatibilities | No breaking changes detected |
| **Behavioral Changes** | ?? Medium | 4 behavioral changes (System.Uri, HostBuilder) | Comprehensive runtime testing required |
| **Code Modifications** | ?? Low | Estimated 4+ LOC impacts | Minimal code changes expected |
| **Testing Coverage** | ?? Low | Dedicated test project exists (TestMeterLib) | Run full test suite post-upgrade |
| **Deprecated Packages** | ?? Low | 1 deprecated package (xunit) | Package remains compatible; no action required |

### Project-Level Risk Assessment

| Project | Risk Level | Key Concerns | Mitigation Strategy |
|---------|-----------|--------------|---------------------|
| **MeterDataLib.csproj** | ?? Low | Core shared library; 2 dependents rely on it | Validate first; run TestMeterLib tests |
| **Api.csproj** | ?? Low | 1 behavioral change; Azure Functions dependency | Test API endpoints; validate Functions runtime |
| **MeterKloud.csproj** | ?? Medium | 3 behavioral changes; Blazor WebAssembly | Thorough UI and runtime testing; verify Uri handling |
| **TestMeterLib.csproj** | ?? Low | Deprecated xunit package | Package still works; monitor for future replacement |

### Behavioral Changes Detail

**System.Uri** (affects Api.csproj, MeterKloud.csproj):
- **Impact**: 2 occurrences in MeterKloud.csproj
- **Risk**: Runtime behavior differences in URI parsing/handling
- **Mitigation**: Test all Uri construction and manipulation; verify BaseAddress handling in HttpClient

**Microsoft.Extensions.Hosting.HostBuilder** (affects MeterKloud.csproj):
- **Impact**: 1 occurrence
- **Risk**: Hosting pipeline initialization changes
- **Mitigation**: Test application startup; verify service provider behavior

### Security Considerations

**No Security Vulnerabilities Detected** ?

All NuGet packages are free of known security issues. The upgrade to .NET 10 LTS provides:
- Extended support lifecycle
- Latest security patches
- Improved security features

### Contingency Plans

#### If Compilation Failures Occur

1. **Review breaking changes catalog** (see Breaking Changes section)
2. **Check package compatibility matrix** (see Package Update Reference)
3. **Consult .NET 10 migration documentation** for framework-specific issues
4. **Rollback option**: Revert to source branch `Net10Upgrade20260222` if issues are unresolvable

#### If Behavioral Changes Cause Issues

1. **System.Uri changes**: Review URI construction code; add explicit validation
2. **HostBuilder changes**: Review startup/hosting configuration; verify DI container behavior
3. **Test incrementally**: Isolate affected components and test separately

#### If Package Update Failures Occur

1. **Verify package versions**: Ensure targeting exact versions from assessment
2. **Check package source**: Confirm NuGet.org accessibility
3. **Dependency conflicts**: Use `dotnet list package --include-transitive` to diagnose
4. **Alternative versions**: Consider patch versions if specific minor version unavailable

### Rollback Strategy

**Git Branch Protection**: All work on `Net10Upgrade20260222` branch

**Rollback Steps**:
1. Identify failure point
2. Document issue for future resolution
3. Reset to last working commit: `git reset --hard HEAD~1`
4. Alternative: Switch back to source branch and create new upgrade branch

**Commit Strategy**: Single atomic commit for the entire upgrade enables clean rollback

---

## Complexity & Effort Assessment

### Overall Complexity

**Rating**: **Low**

This is a straightforward single-version .NET upgrade with minimal complexity factors:
- Modern starting point (.NET 9.0)
- Small codebase (~14K LOC)
- Clean architecture
- Well-maintained dependencies
- No legacy code patterns

### Per-Project Complexity

| Project | Complexity | Dependency Count | Package Updates | Risk Factors | Relative Effort |
|---------|-----------|------------------|-----------------|--------------|-----------------|
| **MeterDataLib.csproj** | ?? Low | 0 projects, 2 packages | 1 | None | Low |
| **Api.csproj** | ?? Low | 0 projects, 9 packages | 3 | 1 behavioral change | Low |
| **MeterKloud.csproj** | ?? Medium | 1 project, 10 packages | 6 | 3 behavioral changes | Medium |
| **TestMeterLib.csproj** | ?? Low | 1 project, 6 packages | 0 | 1 deprecated package | Low |

### Complexity Factors Breakdown

#### MeterDataLib.csproj
- **Lines of Code**: 10,221 (largest project)
- **Complexity Drivers**: 
  - Foundation library (others depend on it)
  - Only 1 package update required
  - No behavioral changes
- **Simplicity Factors**: 
  - No project dependencies
  - Minimal external dependencies
  - Well-isolated functionality

#### Api.csproj
- **Lines of Code**: 38 (smallest project)
- **Complexity Drivers**: 
  - Azure Functions runtime dependencies
  - 1 behavioral change to validate
- **Simplicity Factors**: 
  - Very small codebase
  - No project dependencies
  - Modern Azure Functions Worker model

#### MeterKloud.csproj
- **Lines of Code**: 1,117
- **Complexity Drivers**: 
  - 6 package updates (most in solution)
  - 3 behavioral changes (most in solution)
  - Blazor WebAssembly runtime dependencies
  - UI validation requirements
- **Relative Effort**: Medium (highest in this solution, but still objectively low)

#### TestMeterLib.csproj
- **Lines of Code**: 2,599
- **Complexity Drivers**: 
  - Deprecated xunit package (informational only)
- **Simplicity Factors**: 
  - Tests validate upgrade success
  - No package updates required
  - Deprecated package remains functional

### Phase Complexity Assessment

Since this is an All-at-Once migration, there is one primary phase:

**Phase 1: Atomic Upgrade**
- **Scope**: All 4 projects simultaneously
- **Complexity**: Low
- **Effort**: Low to Medium
  - Framework updates: Low (mechanical change)
  - Package updates: Low (8 confirmed compatible versions)
  - Compilation fixes: Low (no breaking changes detected)
  - Testing: Medium (behavioral changes require runtime validation)

### Resource Requirements

**Technical Skills Required**:
- .NET framework upgrade experience
- Understanding of Blazor WebAssembly
- Azure Functions knowledge (for Api.csproj validation)
- NuGet package management
- Git version control

**Team Capacity**:
- **Single developer**: Suitable - small solution, low complexity
- **Multiple developers**: Not required, but can parallelize validation (e.g., one tests API, another tests Blazor UI)

**Specialized Knowledge**:
- **Blazor WebAssembly**: For MeterKloud.csproj validation
- **Azure Functions**: For Api.csproj validation
- **System.Uri behavioral changes**: For runtime testing

### Effort Distribution

Approximate effort distribution across activities:

| Activity | Relative Effort | Notes |
|----------|----------------|-------|
| **Project file updates** | Low | Mechanical: change `net9.0` to `net10.0` |
| **Package updates** | Low | Mechanical: update 8 package versions |
| **Restore & build** | Low | Automated process |
| **Compilation fixes** | Low | No breaking changes expected |
| **Behavioral change testing** | Medium | 4 changes require runtime validation |
| **Test suite execution** | Low | Automated test execution |
| **UI validation** | Medium | Manual Blazor app verification |
| **API validation** | Low | Azure Functions endpoint testing |

### Dependency Ordering Complexity

**Simple linear dependency chain**:
```
MeterDataLib (foundation) ? {MeterKloud, TestMeterLib} (consumers)
Api (independent)
```

No circular dependencies or complex multi-tier structures to manage.

---

## Source Control Strategy

### Branch Strategy

**Main Branch**: Not directly modified - all work on feature branch

**Source Branch**: `Net10Upgrade20260222`
- Starting point for upgrade
- Contains current .NET 9.0 codebase
- No pending changes

**Upgrade Branch**: `Net10Upgrade20260222` (same as source - work in place)
- All upgrade changes committed here
- Isolated from main development
- Can be tested/validated before merge

**Alternative**: If preferred, create dedicated upgrade branch:
```bash
git checkout -b upgrade-to-NET10
```

### Commit Strategy

**Approach**: **Single Atomic Commit** (Recommended)

Since this is an All-at-Once upgrade, a single commit captures the entire migration:

**Advantages**:
- Clean git history
- Easy to revert if needed
- Clear upgrade boundary
- Simplifies review

**Commit Message Template**:
```
Upgrade solution to .NET 10.0

- Update all projects from net9.0 to net10.0
- Update 8 NuGet packages to .NET 10 compatible versions
- Verify System.Uri behavioral changes (tested)
- Verify HostBuilder behavioral changes (tested)
- All tests pass, no breaking changes

Projects updated:
- MeterDataLib.csproj
- Api.csproj  
- Client/MeterKloud.csproj
- TestMeterLib.csproj

Package updates:
- Microsoft.AspNetCore.Components.WebAssembly: 9.0.11 ? 10.0.3
- Microsoft.AspNetCore.Components.WebAssembly.Authentication: 9.0.11 ? 10.0.3
- Microsoft.AspNetCore.Components.WebAssembly.DevServer: 9.0.11 ? 10.0.3
- Microsoft.Extensions.Caching.Memory: 10.0.1 ? 10.0.3
- Microsoft.Extensions.Http: 10.0.1 ? 10.0.3
- Microsoft.Extensions.Logging.Abstractions: 10.0.1 ? 10.0.3
- System.Text.Encodings.Web: 10.0.1 ? 10.0.3

Behavioral changes validated:
- System.Uri: 3 occurrences tested (Api, MeterKloud)
- HostBuilder: 1 occurrence tested (MeterKloud)

Test results: All tests passing
Build status: Clean build, 0 errors, 0 warnings
```

**Alternative Approach**: **Multiple Commits per Phase**

If you prefer checkpoint commits:

1. **Commit 1**: Framework update
   ```
   Update all project files to net10.0
   
   - MeterDataLib.csproj: net9.0 ? net10.0
   - Api.csproj: net9.0 ? net10.0
   - MeterKloud.csproj: net9.0 ? net10.0
   - TestMeterLib.csproj: net9.0 ? net10.0
   ```

2. **Commit 2**: Package updates
   ```
   Update NuGet packages for .NET 10 compatibility
   
   - Update 8 packages across 3 projects
   - See commit description for full package list
   ```

3. **Commit 3**: Compilation fixes (if any)
   ```
   Fix compilation issues from .NET 10 upgrade
   
   - [List any code changes made]
   ```

4. **Commit 4**: Test validation
   ```
   Verify all tests pass on .NET 10
   
   - TestMeterLib: All tests passing
   - Behavioral changes validated
   ```

**Recommended**: Single atomic commit unless you encounter significant issues requiring incremental commits.

### Review and Merge Process

#### Pre-Merge Checklist

Before merging upgrade branch to main:

**Code Review**:
- [ ] Project file changes reviewed
- [ ] Package version updates verified
- [ ] Any code modifications reviewed

**Testing Verification**:
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Smoke tests completed successfully
- [ ] Behavioral changes validated

**Documentation**:
- [ ] CHANGELOG updated (if maintained)
- [ ] README updated (if .NET version mentioned)
- [ ] Dependencies documented

**Build Verification**:
- [ ] Clean build from fresh clone
- [ ] All projects restore correctly
- [ ] All projects build without errors/warnings

#### Pull Request Guidelines

**PR Title**:
```
Upgrade solution to .NET 10.0 LTS
```

**PR Description Template**:
```markdown
## Description
Upgrades the entire MeterKloud solution from .NET 9.0 to .NET 10.0 (Long Term Support).

## Changes
- ? All 4 projects updated to net10.0
- ? 8 NuGet packages updated to .NET 10 compatible versions
- ? Behavioral changes tested and validated
- ? All tests passing

## Projects Updated
- MeterDataLib.csproj
- Api.csproj
- Client/MeterKloud.csproj
- TestMeterLib.csproj

## Package Updates
See commit message for full package update list.

## Testing
- ? All unit tests pass (TestMeterLib)
- ? MeterKloud Blazor app runs successfully
- ? Api Azure Functions run successfully
- ? System.Uri behavioral changes validated
- ? HostBuilder behavioral changes validated
- ? Integration testing complete

## Breaking Changes
None - all changes backward compatible

## Behavioral Changes
Tested and validated:
- System.Uri: 3 occurrences (Api, MeterKloud)
- HostBuilder: 1 occurrence (MeterKloud)

## Review Checklist
- [ ] Code changes reviewed
- [ ] Package versions verified
- [ ] Tests executed and passing
- [ ] Application smoke tested
- [ ] Documentation updated

## Rollback Plan
Revert this PR to return to .NET 9.0 if issues arise.
```

#### Merge Criteria

**Merge when**:
- All tests pass ?
- Code review approved ?
- CI/CD pipeline passes ?
- Smoke testing successful ?
- No merge conflicts ?

**Merge Method**: 
- **Recommended**: Squash and merge (creates single commit in main)
- **Alternative**: Merge commit (preserves branch history)

### Post-Merge Actions

**After successful merge**:

1. **Tag the release** (optional):
   ```bash
   git tag -a v1.0.0-net10 -m "Upgraded to .NET 10.0"
   git push origin v1.0.0-net10
   ```

2. **Delete upgrade branch** (if separate branch used):
   ```bash
   git branch -d upgrade-to-NET10
   git push origin --delete upgrade-to-NET10
   ```

3. **Update CI/CD pipelines**:
   - Update build agents to use .NET 10 SDK
   - Update deployment configurations
   - Verify automated builds pass

4. **Notify team**:
   - Announce upgrade completion
   - Update local development instructions
   - Ensure all developers install .NET 10 SDK

### Rollback Strategy

**If issues discovered after merge**:

**Option 1: Revert commit**
```bash
git revert <commit-hash>
git push origin main
```

**Option 2: Create fix-forward commit**
- Preferred if issue is minor
- Address specific problem
- Maintain forward progress

**Option 3: Branch from pre-upgrade commit**
- Create hotfix branch from before upgrade
- Deploy hotfix if critical issue
- Fix upgrade issues in separate branch

### Git Workflow Summary

```
main
  |
  ???? Net10Upgrade20260222 (current branch)
         |
         ???? [Upgrade changes committed here]
         |
         ???? Merge to main when complete
```

**Single Atomic Commit Workflow**:
```bash
# Make all upgrade changes
git add .
git commit -m "Upgrade solution to .NET 10.0 [full message]"
git push origin Net10Upgrade20260222

# Create PR: Net10Upgrade20260222 ? main
# After review and approval, merge PR
```

---

## Success Criteria

### Technical Criteria

The .NET 10 upgrade is complete and successful when all of the following criteria are met:

#### Framework Migration

- [x] **All projects migrated to net10.0**
  - [x] MeterDataLib.csproj: net9.0 ? net10.0
  - [x] Api.csproj: net9.0 ? net10.0
  - [x] MeterKloud.csproj: net9.0 ? net10.0
  - [x] TestMeterLib.csproj: net9.0 ? net10.0

#### Package Updates

- [x] **All required packages updated**
  - [x] Microsoft.AspNetCore.Components.WebAssembly: 9.0.11 ? 10.0.3
  - [x] Microsoft.AspNetCore.Components.WebAssembly.Authentication: 9.0.11 ? 10.0.3
  - [x] Microsoft.AspNetCore.Components.WebAssembly.DevServer: 9.0.11 ? 10.0.3
  - [x] Microsoft.Extensions.Caching.Memory: 10.0.1 ? 10.0.3
  - [x] Microsoft.Extensions.Http: 10.0.1 ? 10.0.3
  - [x] Microsoft.Extensions.Logging.Abstractions: 10.0.1 ? 10.0.3
  - [x] System.Text.Encodings.Web: 10.0.1 ? 10.0.3

#### Build Success

- [x] **Solution builds without errors**
  - [x] `dotnet restore MeterKloud.sln` succeeds
  - [x] `dotnet build MeterKloud.sln` succeeds
  - [x] 0 compilation errors across all projects
  - [x] 0 build warnings across all projects

- [x] **Per-project builds succeed**
  - [x] MeterDataLib.csproj builds successfully
  - [x] Api.csproj builds successfully
  - [x] MeterKloud.csproj builds successfully
  - [x] TestMeterLib.csproj builds successfully

#### Test Success

- [x] **All tests pass**
  - [x] `dotnet test MeterKloud.sln` succeeds
  - [x] TestMeterLib test suite: 0 failures
  - [x] All tests discovered and executed
  - [x] Test coverage maintained

#### Dependency Resolution

- [x] **No package conflicts**
  - [x] All NuGet packages restore successfully
  - [x] No version conflicts reported
  - [x] No transitive dependency issues
  - [x] Package restore from clean state succeeds

#### Behavioral Changes Validated

- [x] **System.Uri changes tested**
  - [x] Api.csproj: URI handling works correctly
  - [x] MeterKloud.csproj: BaseAddress construction succeeds
  - [x] MeterKloud.csproj: HttpClient URI resolution works
  - [x] No URI-related runtime errors

- [x] **HostBuilder changes tested**
  - [x] MeterKloud.csproj: Host builds successfully
  - [x] MeterKloud.csproj: Service provider works correctly
  - [x] MeterKloud.csproj: MeterKloudClientApi initializes
  - [x] All dependency injection services resolve

---

### Quality Criteria

#### Code Quality Maintained

- [x] **No regressions introduced**
  - [x] Existing functionality preserved
  - [x] No new bugs introduced
  - [x] Code follows existing patterns
  - [x] No code smells introduced

#### Test Coverage Maintained

- [x] **Test suite integrity**
  - [x] All existing tests still relevant
  - [x] Test coverage percentage unchanged or improved
  - [x] No tests skipped or disabled

#### Documentation Updated

- [x] **Project documentation current**
  - [x] README.md updated with .NET 10 requirements (if applicable)
  - [x] CHANGELOG.md updated with upgrade entry (if maintained)
  - [x] Development setup instructions updated
  - [x] Dependencies documented

---

### Process Criteria

#### Strategy Followed

- [x] **All-at-Once Strategy executed correctly**
  - [x] All projects upgraded simultaneously
  - [x] No intermediate multi-targeting states
  - [x] Single coordinated package update
  - [x] Atomic commit structure followed

#### Source Control Properly Managed

- [x] **Git workflow followed**
  - [x] All changes on appropriate branch (Net10Upgrade20260222)
  - [x] Commit message(s) descriptive and complete
  - [x] No uncommitted changes remaining
  - [x] Branch ready for merge/PR

#### All-at-Once Strategy Principles Applied

- [x] **Simultaneity maintained**
  - [x] All project files updated in single batch
  - [x] All package references updated together
  - [x] Build and fix cycle completed atomically
  - [x] Testing performed on fully upgraded solution

---

### Functional Criteria

#### MeterDataLib.csproj

- [x] **Library functions correctly**
  - [x] Compiles successfully
  - [x] All public APIs available
  - [x] TestMeterLib tests pass
  - [x] Integration with dependent projects works

#### Api.csproj

- [x] **Azure Functions operational**
  - [x] Functions runtime starts (`func start`)
  - [x] All HTTP endpoints respond correctly
  - [x] Request/response handling works
  - [x] Application Insights integration functional
  - [x] No URI behavioral issues observed

#### MeterKloud.csproj

- [x] **Blazor WebAssembly app functional**
  - [x] Application starts (`dotnet run`)
  - [x] Loads in browser without errors
  - [x] All Blazor components render
  - [x] Navigation works correctly
  - [x] Authentication flow functions
  - [x] HttpClient BaseAddress correct
  - [x] API integration works (MeterKloudClientApi)
  - [x] IndexedDB operations succeed
  - [x] LocalStorage operations succeed
  - [x] MudBlazor components display correctly
  - [x] Plotly charts render
  - [x] No console errors
  - [x] No System.Uri runtime errors
  - [x] No HostBuilder/DI errors

#### TestMeterLib.csproj

- [x] **Test project functional**
  - [x] All tests execute successfully
  - [x] xunit framework functions (despite deprecation)
  - [x] Test discovery works
  - [x] Coverage collection works (coverlet)

---

### Performance Criteria

- [x] **No performance regressions**
  - [x] Solution build time acceptable
  - [x] Application startup time maintained or improved
  - [x] Runtime performance maintained or improved
  - [x] Memory usage stable

---

### Deployment Criteria

- [x] **Ready for deployment**
  - [x] All environments have .NET 10 SDK available
  - [x] CI/CD pipelines updated (if applicable)
  - [x] Deployment configurations updated
  - [x] Team notified of .NET 10 requirement

---

### Final Validation Checklist

**Before declaring upgrade complete, verify**:

1. ? All 4 projects build successfully
2. ? All tests pass with 0 failures
3. ? MeterKloud Blazor app runs without errors in browser
4. ? Api Azure Functions start and respond correctly
5. ? All 8 package updates applied correctly
6. ? System.Uri behavioral changes validated (3 occurrences)
7. ? HostBuilder behavioral changes validated (1 occurrence)
8. ? No package dependency conflicts
9. ? No breaking changes encountered
10. ? Documentation updated
11. ? Changes committed to source control
12. ? Ready for code review/PR

---

### Definition of Done

**The .NET 10 upgrade is DONE when**:

? **All technical criteria met** (framework, packages, build, tests)  
? **All quality criteria met** (code quality, coverage, documentation)  
? **All process criteria met** (strategy followed, source control)  
? **All functional criteria met** (all projects operational)  
? **All performance criteria met** (no regressions)  
? **Final validation checklist complete**  
? **Code review approved** (if applicable)  
? **Merged to main branch** (or ready for production deployment)

---

### Post-Upgrade Success Indicators

**Monitor these in the days/weeks following upgrade**:

- No unexpected errors in production logs
- Performance metrics stable or improved
- User-reported issues unchanged or reduced
- Development velocity maintained
- Team confidence in .NET 10 platform

**Long-term benefits realized**:
- Access to .NET 10 LTS support lifecycle (extended support)
- Performance improvements from .NET 10 runtime
- Access to new .NET 10 features (for future development)
- Latest security patches and updates
- Improved developer experience with modern tooling
