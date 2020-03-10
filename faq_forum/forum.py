from faq_forum.create_faqforum import get_all_answered_questions,get_connection

"""
    Give the answer to the most similar question in the database.
"""
def get_answer(question):

    connection = get_connection(r"C:\sqlite\db\faq_forum.db")
    answered_questions = get_all_answered_questions(connection)
    print(list(answered_questions.keys()))
    for (question_to_compare in answered_questions.keys()):
        model_path = "C:/Users/Willem Cossey\\Documents\\GitHub\\P-O-Entrepreneurship-Team-A-code\\nlp_tools\\0001.model"
        #use nlp_tools directory to
    return None


print(get_answer(None))