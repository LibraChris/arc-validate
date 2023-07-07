﻿module TestTasks

open BlackFox.Fake
open Fake.DotNet

open ProjectInfo
open BasicTasks
open System.IO
open Fake.Tools.Git

let ensureArcFixtures = BuildTask.create "EnsureArcFixtures" [] {
    let arcFixtures = [
        "tests/fixtures/arcs/inveniotestarc"
    ]
    arcFixtures
    |> List.iter (fun arcPath ->
        if not (Directory.Exists(arcPath)) then 
            printfn $"cloning {arcPath}"
            Directory.CreateDirectory(arcPath) |> ignore
            Repository.clone "tests/fixtures/arcs" "https://github.com/nfdi4plants/invenio-test-arc" "inveniotestarc"
    )
}

let runTests = BuildTask.create "RunTests" [clean; ensureArcFixtures; build] {
    testProjects
    |> Seq.iter (fun testProject ->
        Fake.DotNet.DotNet.test(fun testParams ->
            {
                testParams with
                    Logger = Some "console;verbosity=detailed"
                    Configuration = DotNet.BuildConfiguration.fromString configuration
                    NoBuild = true
            }
        ) testProject
    )
}

// to do: use this once we have actual tests
let runTestsWithCodeCov = BuildTask.create "RunTestsWithCodeCov" [clean; build] {
    let standardParams = Fake.DotNet.MSBuild.CliArguments.Create ()
    testProjects
    |> Seq.iter(fun testProject -> 
        Fake.DotNet.DotNet.test(fun testParams ->
            {
                testParams with
                    MSBuildParams = {
                        standardParams with
                            Properties = [
                                "AltCover","true"
                                "AltCoverCobertura","../../codeCov.xml"
                                "AltCoverForce","true"
                            ]
                    };
                    Logger = Some "console;verbosity=detailed"
            }
        ) testProject
    )
}