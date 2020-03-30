from profanity_check import predict_prob


def offensiveness(sentence):
    """
    Compute and return the probability that the given sentence is offensive.
    Args:
        sentence: The sentence to check

    Returns: The probability that the given sentence is offensive as a float p (1 = offensive, 0 = nice, 0 <= p <= 1)

    """
    profane_prob = predict_prob([sentence])
    return profane_prob[0]


print(offensiveness("Fuck you"))
print(offensiveness("Little bitch"))
print(offensiveness("You can find a coffee machine on the second floor"))
print(offensiveness("You're so dumb you can't even find a stupid coffee machine"))
print(offensiveness("Can I fire that bitch, Charles?"))
print(offensiveness("Where can I find a coffee machine?"))
print(offensiveness("How can I file a complaint?"))
# Models aren't perfect, this model does not cover all dialects
print(offensiveness("Ray is a cunt"))
