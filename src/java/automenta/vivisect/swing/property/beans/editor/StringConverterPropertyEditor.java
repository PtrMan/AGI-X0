/**
 * L2FProd Common v9.2 License.
 *
 * Copyright 2005 - 2009 L2FProd.com
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package automenta.vivisect.swing.property.beans.editor;

import automenta.vivisect.swing.property.util.converter.ConverterRegistry;

import javax.swing.*;
import javax.swing.text.JTextComponent;


/**
 * StringConverterPropertyEditor. <br>
 * A comma separated list of values.
 */
public abstract class StringConverterPropertyEditor extends AbstractPropertyEditor {

	private Object oldValue;

	public StringConverterPropertyEditor() {

		JTextField text = new JTextField();
		text.setBorder(BorderFactory.createEmptyBorder());

		editor = text;
	}

	@Override
	public Object getValue() {
		String text = ((JTextComponent) editor).getText();
		if (text == null || text.trim().length() == 0) {
			return null;
		} else {
			try {
				return convertFromString(text.trim());
			} catch (Exception e) {
				/* UIManager.getLookAndFeel().provideErrorFeedback(editor); */
				return oldValue;
			}
		}
	}

	@Override
	public void setValue(Object value) {
		if (value == null) {
			((JTextComponent) editor).setText("");
		} else {
			oldValue = value;
			((JTextComponent) editor).setText(convertToString(value));
		}
	}

	protected abstract Object convertFromString(String text);

	protected String convertToString(Object value) {
		return (String) ConverterRegistry.instance().convert(String.class, value);
	}

}
