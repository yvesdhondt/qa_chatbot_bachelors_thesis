from server import faqserver
from nlp_tools import Wrapper_Question_Matching
from faq_forum import auto_moderator

import json


def __get_match(question, question_set):
    """
    Find the best semantic match to question from the given question_set.

    :param question: The question to match
    :param question_set: The questions to match to
    :return: A tuple (p,best) consisting of the 'best' match from the given question_set and the probability,
    'p', that that 'best' match is semantically equal to the given question. (1 = equal, 0 = unequal,
    0 <= p <= 1)
    """
    best_match = None
    best_score = 0
    for q in question_set:
        current_score = Wrapper_Question_Matching.match(question,q)
        if current_score >= best_score
            best_match = q
            best_score = current_score
    return best_score, best_match


def __get_offensiveness(sentence):
    """
    Estimate the offensiveness of a sentence.

    :param sentence: The sentence to estimate the offensiveness of
    :return: the probability,'p', that that the given question is offensive. (1 = yes, 0 = no,
    0 <= p <= 1)
    """
    return auto_moderator.is_offensive(sentence)


def __unwrap_match_request(request):
    """
    Unwrap the given "match questions" request into a tuple of Python objects.

    :param request: A JSON object describing a "match questions" request
    :return: A tuple (question,question_set) where question is a string and question_set is an iterable
    collection of strings.
    """
    return None, None


def __wrap_match_request(question, best_match, prob, identifier):
    """
    Wrap the given result of a "match questions" request in a JSON object

    :param question: The primary question
    :param best_match: The best match
    :param prob: The probability that the two previous questions are semantically the same
    :param identifier: Some integer representing an ID
    :return: A JSON object containing all the given information (as described in faqserver.py)
    """
    ans = {
        "error": "not implemented"
    }

    return json.dumps(ans)


def __unwrap_offensive_request(request):
    """
    Unwrap the given "estimate offensiveness" request into a string.

    :param request: A JSON object describing an "estimate offensiveness" request
    :return: A string that represents the sentence of which to estimate the offensiveness
    """
    return None


def __wrap_offensive_request(question, prob, identifier):
    """
    Wrap the given result of an "estimate offensiveness" request in a JSON object

    :param question: The primary question
    :param prob: The probability that the primary is offensive
    :param identifier: Some integer representing an ID
    :return: A JSON object containing all the given information (as described in faqserver.py)
    """
    ans = {
        "error": "not implemented"
    }

    return json.dumps(ans)


def process(request):
    """
    Process the given request and store the reply in a JSON object.

    :param request: A JSON object describing the request (see faqserver.py for more info)
    :return: The reply to the given request
    """

    """
    Depending on the type of request the correct methods are called to process the request and the reply
    of those methods is then also correctly packaged into a JSON object.
    
    For instance, assume that request is a "match questions" request then
    1. __unwrap_match_request(request) is called to get the question and set of questions to match from
    the JSON object
    2. __get_match(question, question_set) is called to find the best match
    3. __wrap_match_request(question, best_match, prob, identifier) is called to wrap the Python objects 
    into a JSON object
    4. The result of __wrap_match_request(...) is returned
    """

    ans = {
        "error": "not implemented"
    }

    return json.dumps(ans)


def main():
    """
    Go into a while loop and actively wait for requests from the FAQ server

    :return: None
    """

    # Connect to the server
    faq = faqserver.FaqServer()

    # Go to an infinite loop of processing requests of the FAQ forum
    while True:
        # Wait for a request
        while not faq.has_request():
            pass

        # Get the request
        request = faq.get_request()
        # Process the request
        ans = process(request)
        # Answer to the request
        faq.reply(ans)


if __name__ == "__main__":
    main()
