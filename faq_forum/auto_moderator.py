from profanity_check import predict_prob


def is_offensive(sentence):
    profane_prob = predict_prob([sentence])
    return profane_prob[0]


print(is_offensive("Fuck you"))
print(is_offensive("Little bitch"))
print(is_offensive("You can find a coffee machine on the second floor"))
print(is_offensive("You're so dumb you can't even find a stupid coffee machine"))
print(is_offensive("Can I fire that bitch, Charles?"))
print(is_offensive("Where can I find a coffee machine?"))
print(is_offensive("How can I file a complaint?"))
# Models aren't perfect, this model does not cover all dialects
print(is_offensive("Ray is a cunt"))
