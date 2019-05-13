(*
csc /optimize /target:library tests\fsharp\perf\tasks\csbenchmark.cs
artifacts\bin\fsc\Debug\net472\fsc.exe tests\fsharp\perf\tasks\TaskBuilder.fs tests\fsharp\perf\tasks\benchmark.fs --optimize -g -r:csbenchmark.dll
*)

namespace TaskPerf

open System.Threading.Tasks
open System.IO
open TaskBuilderTasks.ContextSensitive // TaskBuilder.fs extension members
open FSharp.Control.ContextSensitiveTasks // the default

module TaskPerfFSharp =
    let private bufferSize = 128
    let manyIterations = 10000

    let syncTask() = Task.FromResult 100
    let syncTask_FSharpAsync() = async.Return 100
    let asyncTask() = Task.Yield()

    let taskBuilder = TaskBuilderTasks.ContextSensitive.task

    let tenBindSync_Task() =
        task {
            let! res1 = syncTask()
            let! res2 = syncTask()
            let! res3 = syncTask()
            let! res4 = syncTask()
            let! res5 = syncTask()
            let! res6 = syncTask()
            let! res7 = syncTask()
            let! res8 = syncTask()
            let! res9 = syncTask()
            let! res10 = syncTask()
            return res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8 + res9 + res10
         }

    let tenBindSync_TaskBuilder() =
        taskBuilder {
            let! res1 = syncTask()
            let! res2 = syncTask()
            let! res3 = syncTask()
            let! res4 = syncTask()
            let! res5 = syncTask()
            let! res6 = syncTask()
            let! res7 = syncTask()
            let! res8 = syncTask()
            let! res9 = syncTask()
            let! res10 = syncTask()
            return res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8 + res9 + res10
         }

    let tenBindSync_FSharpAsync() =
        async {
            let! res1 = syncTask_FSharpAsync()
            let! res2 = syncTask_FSharpAsync()
            let! res3 = syncTask_FSharpAsync()
            let! res4 = syncTask_FSharpAsync()
            let! res5 = syncTask_FSharpAsync()
            let! res6 = syncTask_FSharpAsync()
            let! res7 = syncTask_FSharpAsync()
            let! res8 = syncTask_FSharpAsync()
            let! res9 = syncTask_FSharpAsync()
            let! res10 = syncTask_FSharpAsync()
            return res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8 + res9 + res10
         }

    let tenBindAsync_Task() =
        task {
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
         }

    let tenBindAsync_TaskBuilder() =
        taskBuilder {
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
            do! asyncTask()
         }

    let singleTask_Task() =
        task { return 1 }

    let singleTask_TaskBuilder() =
        taskBuilder { return 1 }

    let singleTask_FSharpAsync() =
        async { return 1 }

    let manyWriteFile_Task () =
        let path = Path.GetTempFileName()
        task {
            let junk = Array.zeroCreate bufferSize
            use file = File.Create(path)
            for i = 1 to manyIterations do
                do! file.WriteAsync(junk, 0, junk.Length)
        }
        |> fun t -> t.Wait()
        File.Delete(path)

    let manyWriteFile_TaskBuilder () =
        let path = Path.GetTempFileName()
        TaskBuilderTasks.ContextSensitive.task {
            let junk = Array.zeroCreate bufferSize
            use file = File.Create(path)
            for i = 1 to manyIterations do
                do! file.WriteAsync(junk, 0, junk.Length)
        }
        |> fun t -> t.Wait()
        File.Delete(path)

    let manyWriteFile_FSharpAsync () =
        let path = Path.GetTempFileName()
        async {
            let junk = Array.zeroCreate bufferSize
            use file = File.Create(path)
            for i = 1 to manyIterations do
                do! Async.AwaitTask(file.WriteAsync(junk, 0, junk.Length))
        }
        |> Async.RunSynchronously
        File.Delete(path)
