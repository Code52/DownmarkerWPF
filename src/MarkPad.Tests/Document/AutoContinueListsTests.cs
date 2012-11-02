using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.Document.EditorBehaviours;
using Xunit;

namespace MarkPad.Tests.Document
{
    public class AutoContinueListsTests
    {

        readonly AutoContinueLists behaviour;

        public AutoContinueListsTests()
        {
             behaviour = new AutoContinueLists();
        }

        [Fact]
        public void ordered_list_match_success_01()
        {
            // arrange                        
            String text = "         1. Some point";

            // act
            var matchOrdered = behaviour.MatchOrderedList(text);
            var matchUnordered = behaviour.MatchUnorderedList(text);

            // assert
            Assert.True(matchOrdered.Success);
            Assert.False(matchUnordered.Success);
            
        }

        [Fact]
        public void ordered_list_match_success_02()
        {
            // arrange                        
            String text = "         10. Some point *italic*";

            // act
            var matchOrdered = behaviour.MatchOrderedList(text);
            var matchUnordered = behaviour.MatchUnorderedList(text);

            // assert
            Assert.True(matchOrdered.Success);
            Assert.False(matchUnordered.Success);

        }

        [Fact]
        public void ordered_list_match_success_03()
        {
            // arrange                        
            String text = "123. Some point **bolded**";

            // act
            var matchOrdered = behaviour.MatchOrderedList(text);
            var matchUnordered = behaviour.MatchUnorderedList(text);

            // assert
            Assert.True(matchOrdered.Success);
            Assert.False(matchUnordered.Success);

        }

        [Fact]
        public void ordered_list_match_faiure_01()
        {
            // arrange                        
            String text = "123 Some point **bolded**";

            // act
            var matchOrdered = behaviour.MatchOrderedList(text);
            var matchUnordered = behaviour.MatchUnorderedList(text);

            // assert
            Assert.False(matchOrdered.Success);
            Assert.False(matchUnordered.Success);

        }

        [Fact]
        public void unordered_list_match_success_01()
        {
            // arrange                        
            String text = "* Some point **bolded**";

            // act
            var matchOrdered = behaviour.MatchOrderedList(text);
            var matchUnordered = behaviour.MatchUnorderedList(text);

            // assert
            Assert.False(matchOrdered.Success);
            Assert.True(matchUnordered.Success);

        }

    }
}
