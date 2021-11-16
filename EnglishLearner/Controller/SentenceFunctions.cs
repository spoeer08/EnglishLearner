﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace EnglishLearner
{
    /*
     * Created by Cole Lamers 
     * Date: 2021-11-04
     * 
     * == Purpose ==
     * Handling of sentence parsing, asserting, and checking
     * 
     * 
     */
    public static class SentenceFunctions
    {
        private static readonly char[] endPunctuation = new char [3] { '.', '?', '!' };

        private static readonly List<string> wordTypes = new List<string> 
        { 
            "n.",      //noun
            "pron.",   //pronoun
            "object",  //another type for pronoun like "I"
            "v.",      //verb
            "adv.",    //adverb
            "imp.",    //Past tense word; -ed
            "p.p.",    //Past tense word; -ed
            "conj.",   //and, both, because, or, then, until, if
            "adj.",    //adjective
            "superl.", //adjective
            "a.",      //adjective
            "prep."    //preposition; on, onto, in, to, through, from, at

        }; // TODO: --1-- consider changing into a dictionary instead so we can return what we want for the values? KISS for word types.

        // TODO: --3-- might want to rename this class because it handles paragraph parsing too

        public static bool Is_Sentence(string consoleInput)
        {
            bool tof = false;
            try
            {
                char[] inputCharArray = consoleInput.ToCharArray();
                if (endPunctuation.Contains(inputCharArray[consoleInput.Length - 1]))
                {
                    tof = true;
                } // if; last character is a period
                else
                {
                    Console.WriteLine("Sentence does not contain a period. Please add it to the end of the sentence."); // required because all future parsing is contingent upon grammar of reading in sentences
                    tof = false;
                } // else
            }
            catch(Exception e)
            {
                UniversalFunctions.LogToFile("Exception:", e);
            }
            return tof;
        } // function Is_Sentence

        public static string[] Split_Paragraph(string paragraph)
        {
            try
            {
                string[] sentences = paragraph.Split('.'); // Splits at period
                for (int i = 0; i < sentences.Length; i++)
                {
                    if (sentences[i].ToCharArray().Equals(' '))
                    {
                        sentences[i] = sentences[i].Remove(0, 1);
                    } // if; removes first char if it's a space
                } // for; loops through all sentences in the paragraph
                return sentences;
            } // try
            catch (Exception e)
            {
                UniversalFunctions.LogToFile("Exception:", e);
                throw e;
            } // catch
        } // function Split_Paragraph

        // TODO: --1-- add a function that replaces & with "and" or + with "plus" or - with "minus" and things like that
        public static (string[], char) GetSplitSentenceAndPunctuation(string sentence)
        {
            UniversalFunctions.LogToFile("GetSplitSentenceAndPunctuation called...");
            /*
             * string[] splitSentence = Regex.Replace(sentence, "[^A-z ]", "").Split(" "); // retain only A-z and spaces
             * passing in char[] is ~2.5x faster at the split join than regex. however consider the regex if things get too complicated or there are additional chars we need to eliminate
             */
            char[] removeChars = new char[] { '\'', '"', '/', ';', ':', '*', '^', '&', '@','\'', '[', ']', '|', '>', '<' }; 
            string[] splitSentence = string.Join("", sentence.Split(removeChars)).Split(" "); //https://stackoverflow.com/questions/7411438/remove-characters-from-c-sharp-string fastest way
            string[] sentenceAsArray = RemoveNullOrWhiteSpaceIndexes(splitSentence);

            return (sentenceAsArray, GetPunctuation(sentenceAsArray));
        } // function GetSplitSentence

        private static string[] RemoveNullOrWhiteSpaceIndexes(string[] splitSentence)
        {
            int j = 0;
            for (int i = 0; i < splitSentence.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(splitSentence[i]))
                {
                    j++;
                } // if; string is not null
            } // // for; each array index

            string[] sentenceAsArray = new string[j];
            int k = 0;
            for (int i = 0; i < splitSentence.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(splitSentence[i]))
                {
                    sentenceAsArray[k] = splitSentence[i];
                    k++; // new array index
                } // if; index is not null
            } // for; each word from the passed in string[]

            return sentenceAsArray;
        } // function RemoveNullOrWhiteSpaceIndexes

        private static char GetPunctuation(string[] sentenceAsArray)
        {
            UniversalFunctions.LogToFile("GetPunctuation called...");

            string finalWord = sentenceAsArray[sentenceAsArray.Length - 1];
            int finalWordPunctuationIndex = finalWord.Length - 1;
            char punctuation = '.';

            if (finalWord.IndexOfAny(endPunctuation) >= 0)
            {
                punctuation = finalWord[finalWordPunctuationIndex];
                finalWord = finalWord.Remove(finalWordPunctuationIndex);
                sentenceAsArray[sentenceAsArray.Length - 1] = finalWord;
            } // if; checks for end sentence punctuation and updates it if it's different and removes it from the split setence

            return punctuation;
        }

        public static char[] GetSeteneceWordTypePattern(string[] splitSentence, Dictionary<string, string[]> sqlAsDict)
        {
            UniversalFunctions.LogToFile("GetSeteneceWordTypePattern called...");
            char[] wordPattern = new char[splitSentence.Length];
            try
            {
                foreach (string word in splitSentence)
                {
                    string[] types;
                    if (sqlAsDict.TryGetValue(word.ToProper(), out types))
                    {
                        types = RemoveNullOrWhiteSpaceIndexes(types); // returns the string array of the word types for that word and removes all empty values
                        for (int i = 0; i < types?.Length; i++)
                        {
                            if(types[i].IndexOfAny(endPunctuation) >= 0)
                            {
                                // TODO: --1-- hunter work on figuring out what wordtype to choose. right here you just need to check each word type item and figure out which one it should be. the if statement is not required but it seems like something we'd need but idk
                            }
                        } // for
                    } // if; key is in dictionary
                } // foreach word
            }
            catch(Exception e)
            {
                UniversalFunctions.LogToFile("Exception", e);
            }

            return wordPattern;
        } // function; GetSeteneceWordTypePattern
    }
}
