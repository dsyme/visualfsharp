// Learn more about F# at http://fsharp.org
open TaskPerf
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open TaskBuilderTasks.ContextSensitive // TaskBuilder.fs extension members
open FSharp.Control.ContextSensitiveTasks

[<MemoryDiagnoser>]
type ManyFileWriteBenchmark() =
    [<Benchmark(Baseline=true)>]
    member __.ManyWriteFile_CSharpAsync () =
        TaskPerfCSharp.ManyWriteFileAsync().Wait();

    [<Benchmark>]
    member __.ManyWriteFile_Task () =
        TaskPerfFSharp.manyWriteFile_Task()

    [<Benchmark>]
    member __.ManyWriteFile_TaskBuilder () =
        TaskPerfFSharp.manyWriteFile_TaskBuilder()

    [<Benchmark>]
    member __.ManyWriteFile_FSharpAsync () =
        TaskPerfFSharp.manyWriteFile_FSharpAsync()

[<MemoryDiagnoser>]
type SyncBindsBenchmark() =
    [<Benchmark(Baseline=true)>]
    member __.SyncBinds_CSharpAsync() = 
         for __ in 1 .. TaskPerfCSharp.ManyIterations*100 do 
             TaskPerfCSharp.TenBindsSync_CSharp().Wait() 

    [<Benchmark>]
    member __.SyncBinds_Task() = 
        for __ in 1 .. TaskPerfFSharp.manyIterations*100 do 
             TaskPerfFSharp.tenBindSync_Task().Wait() 

    [<Benchmark>]
    member __.SyncBinds_TaskBuilder() = 
        for __ in 1 .. TaskPerfFSharp.manyIterations*100 do 
             TaskPerfFSharp.tenBindSync_TaskBuilder().Wait() 

    [<Benchmark>]
    member __.SyncBinds_FSharpAsync() = 
        for i in 1 .. TaskPerfFSharp.manyIterations*100 do 
             TaskPerfFSharp.tenBindSync_FSharpAsync() |> Async.RunSynchronously |> ignore

[<MemoryDiagnoser>]
type AsyncBindsBenchmark() =
    [<Benchmark>]
    member __.AsyncBinds_CSharpAsync() =
         for __ in 1 .. TaskPerfCSharp.ManyIterations do
             TaskPerfCSharp.TenBindsAsync_CSharp().Wait()

    [<Benchmark>]
    member __.AsyncBinds_Task() =
         for __ in 1 .. TaskPerfFSharp.manyIterations do
             TaskPerfFSharp.tenBindAsync_Task().Wait()

    [<Benchmark>]
    member __.AsyncBinds_TaskBuilder() =
         for __ in 1 .. TaskPerfFSharp.manyIterations do
             TaskPerfFSharp.tenBindAsync_TaskBuilder().Wait()

[<MemoryDiagnoser>]
type SingleTaskBenchmark() =
    [<Benchmark>]
    member __.SingleSyncTask_CSharpAsync() = 
         for __ in 1 .. TaskPerfCSharp.ManyIterations*500 do 
             TaskPerfCSharp.SingleSyncTask_CSharp().Wait() 

    [<Benchmark>]
    member __.SingleSyncTask_Task() = 
         for __ in 1 .. TaskPerfFSharp.manyIterations*500 do 
             TaskPerfFSharp.singleTask_Task().Wait() 

    [<Benchmark>]
    member __.SingleSyncTask_TaskBuilder() = 
         for __ in 1 .. TaskPerfFSharp.manyIterations*500 do 
             TaskPerfFSharp.singleTask_TaskBuilder().Wait() 

    [<Benchmark>]
    member __.SingleSyncTask_FSharpAsync() = 
         for __ in 1 .. TaskPerfFSharp.manyIterations*500 do 
             TaskPerfFSharp.singleTask_FSharpAsync() |> Async.RunSynchronously |> ignore

[<EntryPoint>]
let main __ =
    let manyWriteFileResult = BenchmarkRunner.Run<ManyFileWriteBenchmark>()
    let syncBindsResult = BenchmarkRunner.Run<SyncBindsBenchmark>()
    let asyncBindsResult = BenchmarkRunner.Run<AsyncBindsBenchmark>()
    let singleTaskResult = BenchmarkRunner.Run<SingleTaskBenchmark>()

    printfn "%A" manyWriteFileResult
    printfn "%A" syncBindsResult
    printfn "%A" asyncBindsResult
    printfn "%A" singleTaskResult
    0 // return an integer exit code
