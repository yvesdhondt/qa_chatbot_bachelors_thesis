import sqlite3
from sqlite3 import Error


def get_connection(db_file):
    """
    Create a Connection object to the given file
    :param db_file: the database file to connect to
    :return: a Connection to the given database or None if
    something went wrong
    """
    try:
        return sqlite3.connect(db_file)
    except Error as e:
        print(e)
        return None


def create_table(connection, table_text):
    """
    Create a table described by the SQL command in table_text in the
    database given by the connection
    :param connection: a Connection to a database
    :param table_text: an SQL command to create a table
    :return: None
    """
    try:
        cursor = connection.cursor()
        cursor.execute(table_text)
    except Error as e:
        print(e)


def add_answered(connection, question, answer):
    """
    Add the given question-answer pair to the faq forum's answered questions
    :param connection: the connection to a faq forum db
    :param question: the answered question, this CANNOT be None
    :param answer: the answer to the question, this CANNOT be None
    :return: the row id of the inserted question-answer pair
    """
    insert = """ INSERT INTO answered(question,answer)
                 VALUES(?,?) """
    cursor = connection.cursor()
    cursor.execute(insert, (question, answer))
    return cursor.lastrowid


def add_unanswered(connection, question):
    """
    Add the given question to the faq forum's unanswered questions
    :param connection: the connection to a faq forum db
    :param question: the unanswered question, this CANNOT be None
    :return: the row id of the inserted question
    """
    insert = """ INSERT INTO unanswered(question)
                 VALUES(?) """
    cursor = connection.cursor()
    cursor.execute(insert, (question,))
    return cursor.lastrowid


def print_all_unanswered_questions(connection):
    """
    Print all the unanswered questions
    :param connection: the connection to a faq forum db
    :return: None
    """
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM unanswered")

    rows = cursor.fetchall()

    for row in rows:
        print(row)


def print_all_answered_questions(connection):
    """
    Print all the answered question-answer pairs
    :param connection: the connection to a faq forum db
    :return: None
    """
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM answered")

    rows = cursor.fetchall()

    for row in rows:
        print(row)


if __name__ == '__main__':
    # Connect to the faq forum
    faq_forum = get_connection(r"C:\sqlite\db\faq_forum.db")

    # Create the answered and unanswered questions tables
    create_table(faq_forum,
                 """ CREATE TABLE IF NOT EXISTS answered (
                        id integer PRIMARY KEY,
                        question text NOT NULL,
                        answer text NOT NULL
                     ); """)
    create_table(faq_forum,
                 """ CREATE TABLE IF NOT EXISTS unanswered (
                        id integer PRIMARY KEY,
                        question text NOT NULL
                     ); """)

    # Add some test data to the db
    add_answered(faq_forum, "Where is the coffee machine?",
                 "There's one in the break room on the first floor and one in the break room on the second floor.")
    add_answered(faq_forum, "How do I file a complaint?",
                 "You can either talk to someone from HR or file a complaint online at www.filecomplaint.com.")
    add_answered(faq_forum, "How large is Jupyter?",
                 "Jupyter has a radius of around 70.000 km.")

    add_unanswered(faq_forum, "How can I quit my job?")
    add_unanswered(faq_forum, "What is our monthly paper budget?")
    add_unanswered(faq_forum, "What is Yammer?")

    print_all_answered_questions(faq_forum)
    print()
    print_all_unanswered_questions(faq_forum)

    # Close the connection to the db
    faq_forum.close()
