from .create_faqforum import *
from .auto_moderator import is_offensive
from nlp_tools.Wrapper_Question_Matching import check_questions

cut_off = 0.8

# Connect to the faq forum
faq_forum = get_connection(r"C:\sqlite\db\faq_forum.db")

"""
    Give the answer to the most similar question in the database.
""""
def get_answer(question):
    get_connection("C:/sqlite/db")



def get_all_answered_questions(database_name):


def post_question(question):
    """
    Post a question to the forum as long as it is unanswered and not offensive
    
    :param question: the question to post
    """
    # Do not accept offensive questions
    if is_offensive(question):
        return

    answered_questions = get_all_answered_questions(faq_forum)
    for ans_question in answered_questions:
        # Ignore questions that already have an answer
        if check_questions(question, ans_question) >= cut_off:
            return

    add_unanswered(faq_forum, question)
