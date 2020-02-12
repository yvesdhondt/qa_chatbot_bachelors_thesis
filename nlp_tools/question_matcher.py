# This code is based on/inspired by a tutorial from Packt: https://hub.packtpub.com/use-tensorflow-and-nlp-to-detect-duplicate-quora-questions-tutorial/
# The data in "quora_duplicate_questions.tsv" is released for non-commercial use only
# More info can be found on: https://www.quora.com/about/tos
import pandas as pd
import numpy as np

# Read in the data and remove unnecessary columns
data = pd.read_csv("quora_duplicate_questions.tsv", sep="\t").drop(["id", "qid1", "qid2"], axis=1)

# QUANTITATIVE COMPARISON MEASURES

# Calculate the length of each sentence
data["len_q1"] = data.question1.apply(lambda x: len(str(x)))
data["len_q2"] = data.question2.apply(lambda x: len(str(x)))
# Calculate the difference between the lengths of each pair of questions
data["dif_len"] = data.len_q1 - data.len_q2

# Calculate the character length of each sentence (excluding spaces)
data["len_char_q1"] = data.question1.apply(lambda x: len(str(x).replace(" ", "")))
data["len_char_q2"] = data.question2.apply(lambda x: len(str(x).replace(" ", "")))

# Calculate the word count of each sentence
data["len_word_q1"] = data.question1.apply(lambda x: len(str(x).split()))
data["len_word_q2"] = data.question2.apply(lambda x: len(str(x).split()))

# Count the number of common words in each pair of questions
# EXTENSION/UPGRADE try to use synonym libraries to improve this measure
#
data["common_words"] = data.apply(lambda x: len(set(str(x["question1"]).lower().split()).intersection(
                                                set(str(x["question2"]).lower().split()))),
                                  axis=1)

# The quantitative feature set for future reference
fs_1 = ['len_q1', 'len_q2', 'diff_len', 'len_char_q1',
        'len_char_q2', 'len_word_q1', 'len_word_q2',
        'common_words']

# QUALITATIVE COMPARISON MEASURES

