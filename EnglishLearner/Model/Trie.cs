﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EnglishLearner
{
    /*
     * Created by Cole Lamers 
     * Date: 2021-11-04
     * 
     * == Purpose ==
     * To store sentence memories of sentences it has been taught. Future thoughts
     * are to store additional data for each node of word information so that the 
     * Trie can do DFS or BFS traversal looking for additional words it could
     * supplement in the event of an illegal or improper word.
     * 
     * Requires at least one word to traverse
     */
    [Serializable]
    class Trie
    {
        public int MaxNodeLevel = 0;
        public TrieNode Root { get; private set; } // TODO: --4-- fix how this works. Key should pull like a null or something and only child keys from that location. so nodedepth 0 shouldn't be anything

        #region Trie Generation
        public Trie(Phrase currentPhrase)
        {
            UniversalFunctions.LogToFile("Trie Constructor called...");

            this.Root = new TrieNode(currentPhrase.First_Word, 0, currentPhrase.SentencePattern[0]);
            TrieNode Current = this.Root; // TODO: --4-- can most likely add the dictionary here instead of having it declared elsewhere
            TrieNode Next = null;

            for (int i = 1; i < currentPhrase.Split_Sentence.Length; i++)
            {
                TrieNode newNode = new TrieNode(currentPhrase.Split_Sentence[i], i, currentPhrase.SentencePattern[i]);
                Current.Children.Add(newNode.Word, newNode);
                Current.Children.TryGetValue(currentPhrase.Split_Sentence[i], out Next);

                while (Next != null)
                {
                    Current = Next;
                    Current.Children.TryGetValue(currentPhrase.Split_Sentence[i], out Next);
                } // while
            } // for; word in a sentence

            if (Next == null)
            {
                Current.CanBeLastWord = true;
                Current.TypesOfPunctuation += currentPhrase.Punctuation;

                if (this.MaxNodeLevel < Current.NodeDepth)
                {
                    this.MaxNodeLevel = Current.NodeDepth;
                }
            }
        } // Constructor

        public void Append(Phrase currentPhrase)
        {
            DFS_Append(currentPhrase, this.Root);
        } // function Append

        private void DFS_Append(Phrase currentPhrase, TrieNode Current)
        {
            Current.WordFrequency++;
            if (currentPhrase.Split_Sentence.Length > Current.NodeDepth + 1)
            {
                TrieNode Next = null;
                Current.Children.TryGetValue(currentPhrase.Split_Sentence[Current.NodeDepth + 1], out Next);
                // TODO: --2-- can combine this with the trie creation possibly?

                if (Next != null)
                { // recursively dive through Trie
                    DFS_Append(currentPhrase, Next);
                }
                else
                {
                    for (int i = Current.NodeDepth + 1; i < currentPhrase.Split_Sentence.Length; i++)
                    {
                        TrieNode newNode = new TrieNode(currentPhrase.Split_Sentence[i], i, currentPhrase.SentencePattern[i]);
                        Current.Children.Add(newNode.Word, newNode);
                        Next = Current.Children[newNode.Word];

                        while (Next != null)
                        {
                            Current = Next;
                            Current.Children.TryGetValue(currentPhrase.Split_Sentence[i], out Next);
                        } // while
                    } // for; word in a currentPhrase.Split_Sentence

                    if (Next == null)
                    { // Final Node level updates
                        Current.CanBeLastWord = true;
                        Current.TypesOfPunctuation += (Current.TypesOfPunctuation.Contains(currentPhrase.Punctuation) ? "" : currentPhrase.Punctuation);

                        if (this.MaxNodeLevel < Current.NodeDepth)
                        {
                            this.MaxNodeLevel = Current.NodeDepth;
                        }
                    } // if
                } // else; append Trie 
            } // if
            else if (Current.CanBeLastWord == true)
            { // adds another ending punctuation if the same sentence with a different ending punctuation comes up
                Current.TypesOfPunctuation += (Current.TypesOfPunctuation.Contains(currentPhrase.Punctuation) ? "" : currentPhrase.Punctuation);
            }
        } // function DFS_Traversal; starts after root is chosen elsewhere


        #endregion Trie Generation

        #region Breadth Search
        public static void UpdateWordTypes(Dictionary<string, Trie> trieDict, TrieNode node)
        { // Due to the nature of how I have the trieDict set up, I have to do a DFS/BFS hybrid. DFS by Trie, but BFS by node. Hence the two foreach loops.
            //Reset_Trie_Touches(trieDict);
            Queue<TrieNode> bfs_queue = new Queue<TrieNode>();

            foreach (string tKey in trieDict.Keys)
            { // updates the trie roots
                if (tKey.Equals(node.Word))
                {
                    trieDict[tKey].Root.WordType = node.WordType;
                }

                bfs_queue.Enqueue(trieDict[tKey].Root);

                while (bfs_queue.Count > 0)
                {
                    var dequed = bfs_queue.Dequeue();

                    foreach (string childKey in dequed.Children.Keys)
                    { // update the remaining trie children
                        if (dequed.Children[childKey].Word.Equals(node.Word))
                        {
                            dequed.Children[childKey].WordType = node.WordType;
                        }
                        bfs_queue.Enqueue(dequed.Children[childKey]);
                    } // each Queue item
                } // while
            } // foreach
        } // function Update All Words 

         public static string ReturnRandomSentenceFromPattern(Dictionary<string, Trie> trieDict, char[] sentencePattern)
        { // Due to the nature of how I have the trieDict set up, I have to do a DFS/BFS hybrid. DFS by Trie, but BFS by node. Hence the two foreach loops.
            //Reset_Trie_Touches(trieDict);
            Queue<TrieNode> bfs_queue = new Queue<TrieNode>();
            int index = 0;
            WordTypeCollections wtc = new WordTypeCollections();
            
            foreach (string tKey in trieDict.Keys)
            { // updates the trie roots
                var n = wtc.DictOfLists[trieDict[tKey].Root.WordType.ToUpper().ToCharArray()[0]];
                var z = wtc.DictOfLists[trieDict[tKey].Root.WordType.ToUpper().ToCharArray()[0]].IndexOf(trieDict[tKey].Root.Word);
                bool k = z < 0;
                if (wtc.DictOfLists[trieDict[tKey].Root.WordType.ToUpper().ToCharArray()[0]].IndexOf(trieDict[tKey].Root.Word) < 0)
                { // Just avoids duplicates
                    wtc.DictOfLists[trieDict[tKey].Root.WordType.ToUpper().ToCharArray()[0]].Add(trieDict[tKey].Root.Word);
                }

                bfs_queue.Enqueue(trieDict[tKey].Root);

                while (bfs_queue.Count > 0)
                {
                    var dequed = bfs_queue.Dequeue();

                    foreach (string childKey in dequed.Children.Keys)
                    { // update the remaining trie children
                        if (wtc.DictOfLists[dequed.WordType.ToUpper().ToCharArray()[0]].IndexOf(dequed.Word) < 0)
                        { // Just avoids duplicates
                            wtc.DictOfLists[dequed.WordType.ToUpper().ToCharArray()[0]].Add(dequed.Word);
                        }
                        bfs_queue.Enqueue(dequed.Children[childKey]);
                    } // each Queue item
                } // while
            } // foreach

            
            return wtc.GetRandomlyGeneratedSentence(sentencePattern);
        } // function Update All Words 

        public static void DeleteAndReplace(Dictionary<string, Trie> trieDict, Phrase zPhrase, string wordToRemove, string replaceWithThisWord)
        { // Due to the nature of how I have the trieDict set up, I have to do a DFS/BFS hybrid. DFS by Trie, but BFS by node. Hence the two foreach loops.
            //Reset_Trie_Touches(trieDict);

            // TODO: --4-- fix nodes as they come in 
            foreach (string tKey in trieDict.Keys)
            { // updates the trie roots; have to correct children of primary roots before we can adjust the roots
                Queue<TrieNode> bfs_queue = new Queue<TrieNode>();
                bfs_queue.Enqueue(trieDict[tKey].Root);

                while (bfs_queue.Count > 0)
                {
                    var onDeck = bfs_queue.Dequeue();
                    
                    string removeNodeKey = "";
                    string findWordType = "";

                    for (int i = 0; i < zPhrase.Split_Sentence.Length; i++)
                    {
                        if (zPhrase.Split_Sentence[i].ToLower().Equals(wordToRemove.ToLower()))
                        {
                            findWordType = zPhrase.SentencePattern[i];
                        }
                    }

                    TrieNode swapNode = new TrieNode(replaceWithThisWord, onDeck.NodeDepth, findWordType);
                    foreach (KeyValuePair<string, TrieNode> childKey in onDeck.Children)
                    {
                        if (childKey.Key.ToLower().Equals(wordToRemove.ToLower()))
                        {
                            foreach (KeyValuePair<string, TrieNode> grandChildKey in childKey.Value.Children)
                            {
                                swapNode.Children.Add(grandChildKey.Key, grandChildKey.Value);
                            }
                            swapNode.Children = childKey.Value.Children;
                            removeNodeKey = childKey.Key;
                        }
                        else
                        {
                            swapNode.Children.Add(childKey.Key, childKey.Value);
                            bfs_queue.Enqueue(onDeck.Children[childKey.Key]);
                        }
                    }

                    //TODO: --3-- just need to fix the replacement word thing and it should be all set to go.
                    if (swapNode != null)
                    { // add new node once done iterating, then enque it
                        onDeck.Children.Add(replaceWithThisWord, swapNode);
                        bfs_queue.Enqueue(onDeck.Children[replaceWithThisWord]);
                    }

                    if (!removeNodeKey.Equals(""))
                    {
                        onDeck.Children.Remove(removeNodeKey);
                    }
                } // while
            } // foreach


            Trie swapTrie = new Trie(zPhrase);
            string removeTrieKey = "";
/*
            foreach (KeyValuePair<string, Trie> childKey in trieDict)
            {
                if (childKey.Key.ToLower().Equals(wordToRemove.ToLower()))
                {
                    swapTrie.Root.Children.Add(replaceWithThisWord);

                    swapTrie.Enqueue(childKey.Value);
                    removeTrieKey = childKey.Key;
                }
                else
                {
                    swapTrie.Root.Children.Add(childKey.Key, childKey.Value);
                }
            }

            while (swapTrie.Count > 0)
            { // add new node once done iterating, then enque it
                char capitalFirstLetter = replaceWithThisWord.ToUpper().ToCharArray()[0]; // Capitalize First letter
                string modulusCharacters = replaceWithThisWord.Remove(0, 1);              // Remove first character from residual letters
                string bfs_replacementword = capitalFirstLetter + modulusCharacters;      // Combine capitalized with original set

                trieDict.Add(bfs_replacementword, swapTrie.Dequeue());
            }

            while (removeTrieKey.Count > 0)
            { // remove keys
                trieDict.Remove(removeKeys.Dequeue());
            }*/


        } // function Update All Words 

        private static void BFS_UpdateAllWords(TrieNode whichNode, string wordToUpdate, string wordTypeToUpdate)
        {
            Queue<TrieNode> bfs_queue = new Queue<TrieNode>();
            bfs_queue.Enqueue(whichNode);

            while (bfs_queue.Count > 0)
            {
                var dequed = bfs_queue.Dequeue();

                foreach (string childKey in dequed.Children.Keys)
                { // Update all child nodes at the key
                    if (dequed.Children[childKey].Word.Equals(wordToUpdate))
                    {
                        dequed.Children[childKey].WordType = wordTypeToUpdate;
                    }
                    bfs_queue.Enqueue(dequed.Children[childKey]);
                }
            }
        }

        #endregion Breadth Search

        #region Depth Search

        /// <summary>
        /// Update a node by choosing the specific function
        /// </summary>
        /// <param name="fnNode"></param>
        /// <param name="updateWhat">Choice of "wordtype" or "node" and it will update accordingly</param>
        public void Update_Node(FindNode fnNode)
        {
            DFS_Update_Node(fnNode, this.Root);
        } // function Find
        /*
         * TODO: --4-- hold onto for now but i don't know that i'll keep this
                private void DFS_Update_WordType(FindNode fnNode, TrieNode whichNode)
                {
                    UniversalFunctions.LogToFile("DFS_FW called...");

                    if (whichNode.NodeDepth < fnNode.Index && fnNode.Index != 0)
                    { // dive down
                        DFS_Update_WordType(fnNode, whichNode.Children[fnNode.Phrase.Split_Sentence[whichNode.NodeDepth + 1]]);
                    }
                    else
                    { // update node
                        whichNode.WordType = fnNode.NodeMemberPayload.ToString();
                    }
                } // function DFS_Find_Word
        */
        private void DFS_Update_Node(FindNode fnNode, TrieNode whichNode)
        {
            UniversalFunctions.LogToFile("DFS_FW called...");
            try
            {
                if (whichNode.NodeDepth + 1 < fnNode.NodePayload.NodeDepth && fnNode.NodePayload.NodeDepth != 0)
                { // dive down
                    DFS_Update_Node(fnNode, whichNode.Children[fnNode.SentenceArray[whichNode.NodeDepth + 1]]);
                }
                else
                { // update node
                    whichNode = fnNode.NodePayload;
                }
            }
            catch (Exception e)
            {
                UniversalFunctions.LogToFile("Error", e);
            }

        } // function DFS_Find_Word

        private void DFS_Find_All_Word_Instances(string findThisWord, ref List<TrieNode> listOfNodes, TrieNode whichNode)
        {
            UniversalFunctions.LogToFile("DFS_FAI called...");

            Dictionary<string, TrieNode>.KeyCollection nodeKeys = whichNode.Children.Keys;
            TrieNode Next = null;

            foreach (string key in nodeKeys)
            {
                whichNode.Children.TryGetValue(key, out Next);

                if (Next != null)
                {
                    if (whichNode.Children[key].Word.Equals(findThisWord) || whichNode.Children[key].Word.Contains(findThisWord))
                    {
                        listOfNodes.Add(whichNode.Children[key]);
                    } // if; node equals or contains the word
                    DFS_Find_All_Word_Instances(findThisWord, ref listOfNodes, Next);
                    // TODO: --3-- add a delegate function in here? that way if we want to, we can pass in different tasks like "Print everything" or "Find all of these words" or "find the first instance of this word" or "get the height of this word"
                } // if; next is not null
            } // foreach; key
        } // function DFS_Find_Word

        private static void DFS_Find_Sentence(LinkedList<TrieNode> llSentence, ref LinkedListNode<TrieNode> lln, TrieNode whichNode)
        { // The reason .RecentlyTouched is in here three times is because it can't be touched
          // until it commits to further recursion or ending it. Otherwise it will always be true.
            UniversalFunctions.LogToFile("DFS_Find_Sentence called...");

            if (whichNode.CanBeLastWord == true)
            {
                if (!whichNode.RecentlyTouched)
                {
                    whichNode.RecentlyTouched = true;
                    lln = new LinkedListNode<TrieNode>(whichNode);
                    llSentence.AddLast(lln);
                }
                else
                {
                    foreach (string key in whichNode.Children.Keys)
                    {
                        DFS_Find_Sentence(llSentence, ref lln, whichNode.Children[key]);
                        if (llSentence.Count > 0) { break; }
                    } // foreach

                    whichNode.RecentlyTouched = true;
                    lln = new LinkedListNode<TrieNode>(whichNode);
                    llSentence.AddLast(lln);
                }
            }
            else
            { // Not a final word from a phrase given
                whichNode.RecentlyTouched = true;
                foreach (string key in whichNode.Children.Keys)
                {
                    DFS_Find_Sentence(llSentence, ref lln, whichNode.Children[key]);
                    if (llSentence.Count > 0) { break; }
                } // foreach
                
                whichNode.RecentlyTouched = true;
                lln = new LinkedListNode<TrieNode>(whichNode);
                llSentence.AddLast(lln);
            }

        } // function DFS_Find_Sentence

        private static void DFS_Find_Sentence_With_SentenceArray(LinkedList<TrieNode> llSentence, ref LinkedListNode<TrieNode> lln, TrieNode whichNode, string[]  sentenceArray)
        { // The reason .RecentlyTouched is in here three times is because it can't be touched
          // until it commits to further recursion or ending it. Otherwise it will always be true.
            UniversalFunctions.LogToFile("DFS_Find_Sentence_With_Phrase called...");
 
            if (whichNode.NodeDepth + 1 < sentenceArray.Length) // whichNode.NodeDepth + 1 = the first word after the root word initially passed in
            {
                TrieNode Next = null;
                if (whichNode.Children.Keys.Count > 0)
                {
                    whichNode.Children.TryGetValue(sentenceArray[whichNode.NodeDepth + 1], out Next);
                    DFS_Find_Sentence_With_SentenceArray(llSentence, ref lln, Next, sentenceArray);
                }
            }

            whichNode.RecentlyTouched = true;
            lln = new LinkedListNode<TrieNode>(whichNode);
            llSentence.AddLast(lln);
        } // function DFS_Find_Sentence


        #endregion Depth Search

        public List<TrieNode> Find_All_Instances(string findThisWord, bool dfs_default = true)
        {
            List<TrieNode> listOfNodes = new List<TrieNode>();
            // TODO: --3-- idk what todo but this isn't done
            if (dfs_default)
            {
                DFS_Find_All_Word_Instances(findThisWord, ref listOfNodes, this.Root);
            }
            else
            {
                //BFS_FAI(findThisWord, this.Root);
            }
            return listOfNodes;
        }

        private static void ResetTrieTouchedValues(TrieNode whichNode)
        {
            UniversalFunctions.LogToFile("ResetTrieTouchedValues called...");
            whichNode.RecentlyTouched = false;
            TrieNode Next = null;

            foreach (string key in whichNode.Children.Keys)
            {
                whichNode.Children.TryGetValue(key, out Next);
                ResetTrieTouchedValues(Next);
            } // foreach
        }

        /// <summary>
        /// Resets all touched values to false via DFS. USE THIS ONE FOR RESETTING!
        /// </summary>
        /// <param name="trieDict"></param>
        public static void Reset_Trie_Touches(Dictionary<string, Trie> trieDict)
        {
            foreach (string key in trieDict.Keys)
            { // resets trie for searching
                ResetTrieTouchedValues(trieDict[key].Root);
            }
        } // function Reset_Trie_Touches

        public static LinkedListNode<TrieNode> Get_Sentence_As_LinkedList(Dictionary<string, Trie> trieDict, string[] sentenceArray)
        { // Returns based off an exact sentence passed in

            LinkedList<TrieNode> llSentence = new LinkedList<TrieNode>();
            LinkedListNode<TrieNode> lln = null;

            Trie tempTrie = null;
            trieDict.TryGetValue(sentenceArray[0].ToProper(), out tempTrie);

            if (tempTrie != null)
            {
                DFS_Find_Sentence_With_SentenceArray(llSentence, ref lln, tempTrie.Root, sentenceArray);
                if (llSentence.Count > 0)
                {
                    while (lln.Previous != null)
                    {
                        lln = lln.Previous;
                    }
                }
            }

            return lln;
        } // function Get_Sentence_As_LinkedList

        public static LinkedListNode<TrieNode> Get_Sentence_As_LinkedList(Dictionary<string, Trie> trieDict)
        { // gets first instance on untouched endnodes
            LinkedListNode<TrieNode> lln = null;
            var newllnList = new LinkedList<TrieNode>();
            bool notFoundYet = false;

            foreach (string tKey in trieDict.Keys)
            { // finds a linkedlist 
                newllnList = null;
                newllnList = new LinkedList<TrieNode>();

                DFS_Find_Sentence(newllnList, ref lln, trieDict[tKey].Root);
                if (newllnList.Count > 0)
                {
                    var temp = lln;
                    while (temp != null)
                    {
                        if (temp.Value.WordType.Equals("?"))
                        {
                            notFoundYet = true;
                            break;
                        }
                        temp = temp.Previous;
                    } // while
 
                } // if

                if (notFoundYet)
                {
                    break;
                }
            } // foreach
/*
 * Solely for debugging purposes to get the string
            string testing = "";
            var test = lln;
            while (test != null)
            {
                testing += test.Value.Word + " ";
                test = test.Previous;
            }
*/
            return lln;
        } // function Get_Sentence_As_LinkedList

    } // Class Trie
} // namespace
