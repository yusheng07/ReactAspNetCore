/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react';
import React from 'react';
import { Page } from './Page';
import { useParams } from 'react-router-dom';
import { getQuestion, /*QuestionData,*/ postAnswer } from './QuestionsData';
import { AnswerList } from './AnswerList';
import { gray3, gray6, Fieldset, FieldContainer, FieldLabel, FieldError,
  FieldTextArea, FormButtonContainer, PrimaryButton, SubmissionSuccess } from './Styles';
import { useForm } from 'react-hook-form';
import { useSelector, useDispatch } from 'react-redux';
import { gettingQuestionAction, gotQuestionAction, AppState } from './Store';
import { useAuth } from './Auth';

type FormData = {
  content: string;
};

export const QuestionPage = () => {
  const dispatch = useDispatch();
  const question = useSelector(
    (state: AppState) => state.questions.viewing,
  );
  const [successfullySubmitted, setSuccessfullySubmitted] = React.useState(false);
  // const [question, setQuestion] = React.useState<QuestionData | null>(null);
  const { questionId } = useParams();
  React.useEffect(() => {
    const doGetQuestion = async (questionId: number) => {
      dispatch(gettingQuestionAction());
      const foundQuestion = await getQuestion(questionId);
      dispatch(gotQuestionAction(foundQuestion));
      // setQuestion(foundQuestion);
    };
    if (questionId) {
      doGetQuestion(Number(questionId));
    }
  }, [questionId]);
  const {
    register,
    formState: { errors, isSubmitting },
    handleSubmit,
  } = useForm<FormData>({ mode: 'onBlur' });
  const submitForm = async (data: FormData) => {
    const result = await postAnswer({
      questionId: question!.questionId,
      content: data.content,
      userName: 'Fred',
      created: new Date(),
    });
    setSuccessfullySubmitted(result ? true : false);
  };

  const { isAuthenticated } = useAuth();

  return (
    <Page>
      <div
        css={css`
          background-color: white;
          padding: 15px 20px 20px 20px;
          border-radius: 4px;
          border: 1px solid ${gray6};
          box-shadow: 0 3px 5px 0 rgba(0, 0, 0, 0.16);
        `}
      >
        <div
          css={css`
            font-size: 19px;
            font-weight: bold;
            font-weight: bold;
          `}
        >
          {question === null ? '' : question.title}
        </div>
        {question !== null && (
          <React.Fragment>
            <p
              css={css`
                margin-top: 0px;
                background-color: white;
              `}
            >
              {question.content}
            </p>
            <div
              css={css`
                font-size: 12px;
                font-style: italic;
                color: ${gray3};
              `}
            >
              {`Asked by ${question.userName} on 
                ${question.created.toLocaleDateString()} ${question.created.toLocaleTimeString()}`}
            </div>
            <AnswerList data={question.answers} />
            {isAuthenticated && (
              <form
                onSubmit={handleSubmit(submitForm)}
                css={css`
                  margin-top: 20px;
                `}
              >
                <Fieldset disabled={isSubmitting || successfullySubmitted}>
                  <FieldContainer>
                    <FieldLabel htmlFor="content">Your Answer</FieldLabel>
                    <FieldTextArea
                      {...register('content', { required: true, minLength: 50 })}
                      id="content"
                    />
                    {errors.content && errors.content.type === 'required' && (
                      <FieldError>You must enter the answer</FieldError>
                    )}
                    {errors.content && errors.content.type === 'minLength' && (
                      <FieldError>The answer must be at least 50 characters</FieldError>
                    )}
                  </FieldContainer>
                  <FormButtonContainer>
                    <PrimaryButton type="submit">Submit Your Answer</PrimaryButton>
                  </FormButtonContainer>
                  {successfullySubmitted && (
                    <SubmissionSuccess>
                      Your answer was successfully submitted
                    </SubmissionSuccess>
                  )}
                </Fieldset>
              </form>
            )}
          </React.Fragment>
        )}
      </div>
    </Page>
  );
};
