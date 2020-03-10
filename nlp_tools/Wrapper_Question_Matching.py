import sys
from fuzzywuzzy import fuzz
import gensim
import numpy as np
#import nltk
from nltk import word_tokenize
from nltk.corpus import stopwords
stop_words = set(stopwords.words("english"))
from scipy.spatial.distance import cosine, cityblock, jaccard, canberra, euclidean, minkowski, braycurtis

# Program takes two strings as input containing the questions to be compared
question_1 = sys.argv[1]
question_2 = sys.argv[2]
W2VModel = gensim.models.KeyedVectors.load_word2vec_format("GoogleNews-vectors-negative300-SLIM.bin.gz", binary=True)

# 1x uitvoeren
# nltk.download("punkt")
# nltk.download("stopwords")

"""
Method getFeatures(question1, question2) returns dictionary of features given two questions
"len_q1"                            =   length of the first string
"len_q2"                            =   length of second string
"diff_len"                          =   difference in length (len_q1-len_q2)
"len_char_q1"                       =   length of the first string without the spaces
"len_char_q2"                       =   length of the second string without the spaces
"len_word_q1"                       =   word count of the first string
"len_word_q2"                       =   word count of the second string
"common_words"                      =   count of words the two strings have in common
"fuzz_Qratio"                       =   Q ratio of the strings
"fuzz_Wratio"                       =   W ratio of the string
"fuzz_partial_ratio"                =   partial ratio of the strings
"fuzz_partial_token_set_ratio"      =   partial token set ratio
"fuzz_partial_token_sort_ratio"     =   partial token sort ratio
"fuzz_token_set_ratio"              =   token set ratio
"fuzz_token_sort_ratio"             =   token sort ratio
"""


def getFeatures(question1, question2):
    W2V = word2vecFeatures(question1, question2, W2VModel)
    outputDict = {
        # length based features
        "len_q1": len(question1),
        "len_q2": len(question2),
        "diff_len": len(question1) - len(question2),
        "len_char_q1": len(question1.replace(" ", "")),
        "len_char_q2": len(question2.replace(" ", "")),
        "common_words": len(set(question1.lower().split()).intersection(set(question2.lower().split()))),
        # distance based features
        #   (fuzzywuzzy library tutorial: https://www.datacamp.com/community/tutorials/fuzzy-string-python)
        "fuzz_Qratio": fuzz.QRatio(question1, question2),
        "fuzz_Wratio": fuzz.WRatio(question1, question2),
        "fuzz_partial_ratio": fuzz.partial_ratio(question1, question2),
        "fuzz_partial_token_set_ratio": fuzz.partial_token_set_ratio(question1, question2),
        "fuzz_partial_token_sort_ratio": fuzz.partial_token_sort_ratio(question1, question2),
        "fuzz_token_set_ratio": fuzz.token_set_ratio(question1, question2),
        "fuzz_token_sort_ratio": fuzz.token_sort_ratio(question1, question2),
        # tfidf based features
        # ToDo

        # word2vec based features
        "cosine_distance": cosine(W2V[0], W2V[1]),
        "cityblock_distance": cityblock(W2V[0], W2V[1]),
        "jaccard_distance": jaccard(W2V[0], W2V[1]),
        "canberra_distance": canberra(W2V[0], W2V[1]),
        "euclidean_distance": euclidean(W2V[0], W2V[1]),
        "minkowski_distance": minkowski(W2V[0], W2V[1]),
        "braycurtis_distance": braycurtis(W2V[0], W2V[1]),
        "wmd": W2V[2],
        "norm_wmd": W2V[3]

    }
    return outputDict


"""
Method match takes two questions and returns the probability they mean the same thing
"""


# ToDo
def match(question1, question2):
    pass


def getWmd(question1, question2, model):
    s1 = question1.lower().split()
    s2 = question2.lower().split()
    s1 = [w for w in s1 if w not in stop_words]
    s2 = [w for w in s2 if w not in stop_words]
    return model.wmdistance(s1, s2)


def word2vecFeatures(question1, question2, model):
    # Calculate the sent2vec vectors for every question
    w2v_q1 = np.array(sent2vec(question1, model))
    w2v_q2 = np.array(sent2vec(question2, model))
    wmd = getWmd(question1, question2, model)
    model.init_sims(replace=True)
    norm_wmd = getWmd(question1, question2, model)
    return [w2v_q1, w2v_q2, wmd, norm_wmd]


# Google's Word2vec model expects words as input, so sentences must be
# transformed to vectors indirectly
def sent2vec(s, model):
    words = word_tokenize(s.lower())
    # Stopwords and numbers must be removed, as well as words that are not
    # part of the model
    M = [model[w] for w in words if w not in stop_words and w.isalpha() and w in model]
    M = np.array(M)
    if len(M) > 0:
        v = M.sum(axis=0)
        return v / np.sqrt((v ** 2).sum())
    else:
        # When the sentence is empty after removing unvalid tokens, the vector
        # is equal to the null-vector
        return model.get_vector('null')



# this variable should be written to an output file
outputFeatures = getFeatures(question_1, question_2)
print(outputFeatures)

def match(question1, question2):
    probability = None
    #Load model

    #Process questions into numpy array of features

    #Calculate probability via model

    return probability
