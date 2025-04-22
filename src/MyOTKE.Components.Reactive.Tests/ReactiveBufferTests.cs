using FluentAssertions;
using MyOTKE.Core;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace MyOTKE.Components.Reactive;

public class ReactiveBufferTests
{
    public static IEnumerable<object[]> TestCases
    {
        get
        {
            static object[] MakeTestCase(Action<TestSource> action, ICollection<(int, int)> expectedVertices) =>
                [action, expectedVertices, Enumerable.Range(0, expectedVertices.Count).ToArray()];

            return
            [
                MakeTestCase( // addition, const size
                    a => { a.Add(1, 2); a.Add(2, 2); },
                    [(1, 1), (1, 2), (2, 1), (2, 2)]),

                MakeTestCase( // removal from middle, const size
                    a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(0); },
                    [(2, 1), (2, 2)]),

                MakeTestCase( // removal from end, const size
                    a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(1); },
                    [(1, 1), (1, 2)]),

                MakeTestCase( // replacement, const size
                    a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 2); },
                    [(3, 1), (3, 2), (2, 1), (2, 2)]),

                MakeTestCase( // clear
                    a => { a.Add(1, 2); a.Add(2, 2); a.Clear(); a.Add(3, 2); },
                    [(3, 1), (3, 2)]),

                MakeTestCase( // addition, varying sizes
                    a => { a.Add(1, 4); a.Add(2, 2); },
                    [(1, 1), (1, 2), (1, 3), (1, 4), (2, 1), (2, 2)]),

                MakeTestCase( // removal, varying sizes
                    a => { a.Add(1, 2); a.Add(2, 4); a.RemoveAt(0); },
                    [(2, 3), (2, 4), (2, 1), (2, 2)]),

                MakeTestCase( // replacement, varying sizes - bigger
                    a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 4); },
                    [(3, 1), (3, 2), (2, 1), (2, 2), (3, 3), (3, 4)]),

                MakeTestCase( // replacement, varying sizes - smaller
                    a => { a.Add(1, 4); a.Add(2, 2); a[0] = (3, 2); },
                    [(3, 1), (3, 2), (2, 1), (2, 2)]),

                MakeTestCase( // replacement at end, varying sizes - smaller
                    a => { a.Add(1, 2); a.Add(2, 4); a[1] = (3, 2); },
                    [(1, 1), (1, 2), (3, 1), (3, 2)]),
            ];
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Test(
        Action<TestSource> action,
        ICollection<(int, int)> expectedVertices,
        ICollection<int> expectedIndices)
    {
        // Arrange
        var sourceObservable = new TestSource();
        var targetVao = new MemoryVertexArrayObject<(int, int)>(
            (BufferUsageHint.DynamicDraw, 100, null),
            (100, null));

        using (var sut = new ReactiveBuffer<(int, int)>(targetVao, sourceObservable, [0, 1]))
        {
            // Act
            action(sourceObservable);
        }

        // Assert
        targetVao.AttributeBuffer1.Content.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices, opts => opts.WithStrictOrdering());
        targetVao.IndexBuffer.Content.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices, opts => opts.WithStrictOrdering());
    }

    public class TestSource : IObservable<IObservable<IList<(int, int)>>>
    {
        private readonly Subject<Subject<IList<(int, int)>>> outerSubject = new();
        private readonly List<Subject<IList<(int, int)>>> innerSubjects = [];

        public (int, int) this[int index]
        {
            set
            {
                innerSubjects[index].OnNext(
                    [.. Enumerable.Range(1, value.Item2).Select(i => (value.Item1, i))]);
            }
        }

        public void Add(int id, int count)
        {
            var innerSubject = new Subject<IList<(int, int)>>();
            outerSubject.OnNext(innerSubject);
            innerSubjects.Add(innerSubject);
            this[innerSubjects.Count - 1] = (id, count);
        }

        public void RemoveAt(int index)
        {
            innerSubjects[index].OnCompleted();
            innerSubjects.RemoveAt(index);
        }

        public void Clear()
        {
            innerSubjects.ForEach(s => s.OnCompleted());
            innerSubjects.Clear();
        }

        public IDisposable Subscribe(IObserver<IObservable<IList<(int, int)>>> observer)
        {
            return outerSubject.Subscribe(observer);
        }
    }
}
