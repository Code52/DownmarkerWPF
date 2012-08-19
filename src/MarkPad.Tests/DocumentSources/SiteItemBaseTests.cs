using System;
using Caliburn.Micro;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources
{
    public class SiteItemBaseTests
    {
        [Fact]
        public void siteitembase_initialises_children()
        {
            // arrange
            var testItem = new TestItem(Substitute.For<IEventAggregator>());

            // act
            var children = testItem.Children;

            // assert
            Assert.Empty(children);
        }

        [Fact]
        public void name_can_be_set_initially()
        {
            // arrange/act
            var testItem = new TestItem(Substitute.For<IEventAggregator>()) {Name = "Test"};

            // assert
            Assert.Equal("Test", testItem.Name);
        }

        [Fact]
        public void isrenaming_throws_when_not_selected()
        {
            // arrange
            var testItem = new TestItem(Substitute.For<IEventAggregator>()); 

            // act

            // assert
            Assert.Throws<InvalidOperationException>(() => testItem.IsRenaming = true);
        }
        
        [Fact]
        public void name_gets_changed_when_item_is_being_renamed()
        {
            // arrange
            var testItem = new TestItem(Substitute.For<IEventAggregator>())
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Name = "Renamed";

            // assert
            Assert.Equal("Renamed", testItem.Name);
        }

        [Fact]
        public void automatically_subscribes_self_to_eventaggregator()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            var testItem = new TestItem(eventAggregator);

            //assert
            eventAggregator.Received().Subscribe(testItem);
        }

        [Fact]
        public void unsubscribes_self_from_event_aggregator_on_dispose()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            var testItem = new TestItem(eventAggregator);

            // act
            testItem.Dispose();

            //assert
            eventAggregator.Received().Unsubscribe(testItem);
        }

        [Fact]
        public void disposes_children_on_dispose()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            var testItem = new TestItem(eventAggregator);
            var child = new TestItem(eventAggregator);
            testItem.Children.Add(child);

            // act
            testItem.Dispose();

            // assert
            Assert.True(child.Disposed);
        }
    }
}