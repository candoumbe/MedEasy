import * as React from "react";
import { Modal, Row, Button, ModalBody } from "react-bootstrap";
import { FormComponent } from "./../FormComponent";
import { FormFieldComponent } from "./../FormFieldComponent";
import { Form } from "./../../restObjects/Form";
import { FormField } from "./../../restObjects/FormField";
import { LoadingComponent } from "./../LoadingComponent";

/** State of the component */
interface DoctorCreatePageState {
    /** The current form displayed */
    form?: Form,
    /** Should a loading component be displayed ? */
    loading: boolean,

    formState?: {
        [key: string]: boolean | number | string | Date | undefined
    }

}

interface DoctorCreateComponentProps {
    /** endpoint where to get forms descriptions from */
    endpoint: string,
    handleChange?: (name: string, value: any) => void;
}



export class DoctorQuickCreateComponent extends React.Component<DoctorCreateComponentProps, DoctorCreatePageState> {

    private readonly form: Form;
    private readonly submit: React.EventHandler<React.FormEvent<HTMLFormElement>>;
    private readonly handleChange: (name: string, value: any) => void;

    /**
     * Builds a new instance
     * @param props
     */
    public constructor(props) {
        super(props);


        this.form = new Form();
        this.form.items = [
            { name: "Name", type: "string", description: "Fullname" }
        ];

        this.submit = async (event) => {
            event.preventDefault();

            let response: Response = await fetch(
                this.props.endpoint,
                {
                    headers: { "content-type": "application/json" },
                    method: "POST",
                    body: JSON.stringify(this.state.form)
                });
            if (!response.ok) {
                let errors = await (response.json() as Promise<Array<MedEasy.DTO.ErrorInfo>>)
                this.setState((prevState, props) => {
                    let newState = Object.assign({}, prevState, { ongoing: false, errors: errors });

                    return newState;
                });
            }
        };
        this.handleChange = (name, val) => {
            this.setState((prevState, props) => {
                let newState = prevState;
                newState.formState[name] = val;
                return newState;
            });
        }

    }


    public render(): JSX.Element {
        
        return <Modal animation onHide={() => { }} draggable>
            <ModalBody>
                {
                    this.state.form
                        ? <FormComponent form={this.state.form} handleSubmit={this.submit} onChange={this.handleChange}>
                            <Row>
                                <Button as="submit" />
                            </Row>
                        </FormComponent>
                        : <LoadingComponent />
                };
            </ModalBody>
        </Modal>
    }
}