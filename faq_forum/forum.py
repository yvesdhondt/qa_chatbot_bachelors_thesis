from server import faqserver
import json
from faq_forum.question_match import match
from faq_forum.auto_moderator import offensiveness


def __get_match(question, question_set):
    """
    Find the best semantic match to question from the given question_set.

    :param question: The question to match
    :param question_set: The questions to match to
    :return: A tuple (p,best) consisting of the 'best' match from the given question_set and the probability,
    'p', that that 'best' match is semantically equal to the given question. (1 = equal, 0 = unequal,
    0 <= p <= 1). If the question_set was empty or None, (0.0, None) is returned
    """
    prob = 0.0
    best = None

    # Only loop over the questions in question_set if it is not None
    if question_set is not None:
        # Find the best match to the given question
        best = max(question_set, key=lambda x: match(question, x))
        prob = match(question, best)

    return prob, best


def __get_offensiveness(sentence):
    """
    Estimate the offensiveness of a sentence.

    :param sentence: The sentence to estimate the offensiveness of
    :return: the probability,'p', that that the given question is offensive. (1 = yes, 0 = no,
    0 <= p <= 1). 0.0 is returned if the given sentence was None.
    """
    if sentence is None:
        return 0.0
    return offensiveness(sentence)


def __unwrap_match_request(request):
    """
    Unwrap the given "match questions" request into a tuple of Python objects.

    :pre: The given request is not None, it is a JSON string (NOT a dict)
    :param request: A JSON object describing a "match questions" request
    :return: A tuple (question,question_set) where question is a string and question_set is an iterable
    collection of strings.
    """
    request = json.loads(request)
    question = request["Primary"]
    question_set = request["Questions"]

    return question, question_set


def __wrap_match_request(question, best_match, prob, identifier):
    """
    Wrap the given result of a "match questions" request in a JSON object

    :param question: The primary question
    :param best_match: The best match
    :param prob: The probability that the two previous questions are semantically the same, a float
    :param identifier: Some integer representing an ID
    :return: A JSON object containing all the given information (as described in faqserver.py)
    """
    ans = {
        "Primary": question,
        "Match": best_match,
        "Prob": str(prob),
        "ID": identifier
    }

    return json.dumps(ans)


def __unwrap_offensive_request(request):
    """
    Unwrap the given "estimate offensiveness" request into a string.

    :param request: A JSON object describing an "estimate offensiveness" request
    :return: A string that represents the sentence of which to estimate the offensiveness
    """
    request = json.loads(request)
    question = request["Primary"]

    return question


def __wrap_offensive_request(question, prob, identifier):
    """
    Wrap the given result of an "estimate offensiveness" request in a JSON object

    :param question: The primary question
    :param prob: The probability that the primary is offensive, a float
    :param identifier: Some integer representing an ID
    :return: A JSON object containing all the given information (as described in faqserver.py)
    """
    ans = {
        "Primary": question,
        "Prob": str(prob),
        "ID": identifier
    }

    return json.dumps(ans)


def process(request):
    """
    Process the given request and store the reply in a JSON object.

    :pre: The given request is not None, it is a JSON string (NOT a dict)
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
    error = {
        "error": "request was None"
    }

    # Process the request
    if request is None:
        ans = error
    else:
        req_dict = json.loads(request)
        if "Action" not in req_dict:
            ans = error
        elif req_dict["Action"] == "match_question":
            inp = __unwrap_match_request(request)
            out = __get_match(inp[0], inp[1])
            ans = __wrap_match_request(inp[0],
                                       out[1],
                                       out[0],
                                       req_dict["ID"])
        elif req_dict["Action"] == "estimate_offensiveness":
            inp = __unwrap_offensive_request(request)
            out = __get_offensiveness(inp)
            ans = __wrap_offensive_request(inp, out, req_dict["ID"])
        else:
            ans = {
                "error": "the given request is not supported"
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


def test():
    req_1 = '{ "Action":"match_question", ' \
            '"Primary":"Where is the coffee machine?",' \
            '"Questions":[' \
            '"Where can I find the coffee machine?",' \
            '"When did the Titanic sink?",' \
            '"How can I use the coffee machine?"' \
            '],' \
            '"ID":"214" }'
    ans_1 = process(req_1)
    print(ans_1)

    req_2 = '{ "Action":"estimate_offensiveness", ' \
            '"Primary":"Charlie is a little bitch, haha, what a little shit :p",' \
            '"ID":"219" }'
    ans_2 = process(req_2)
    print(ans_2)


if __name__ == "__main__":
    # Remove the test() method in deployed scripts, it is purely used for debugging
    test()
    main()
