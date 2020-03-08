import sys

# Program takes two strings as input containing the questions to be compared
question_1 = sys.argv[0]
question_2 = sys.argv[1]

"""
Method getFeatures(question1, question2) returns dictionary of features given two questions
"len_q1"        =   length of the first string
"len_q2"        =   length of second string
"diff_len"      =   difference in length (len_q1-len_q2)
"len_char_q1"   =   length of the first string without the spaces
"len_char_q2"   =   length of the second string without the spaces
"len_word_q1"   =   word count of the first string
"len_word_q2"   =   word count of the second string
"common_words"  =   count of words the two strings have in common
"""


def getFeatures(question1, question2):
    outputDict = {
        "len_q1": len(question1),
        "len_q2": len(question2),
        "diff_len": len(question1) - len(question2),
        "len_char_q1": len(question1.replace(" ", "")),
        "len_char_q2": len(question2.replace(" ", "")),
        "common_words": len(set(question1.lower().split()).intersection(set(question2.lower().split()))),

    }
    return outputDict


# this variable should be written to an output file
outputFeatures = getFeatures(question_1, question_2)
