package automenta.vivisect.swing.property.sheet.renderer;

import automenta.vivisect.swing.property.sheet.I18N;

import javax.swing.*;
import javax.swing.table.TableCellRenderer;
import java.awt.*;
import java.io.Serializable;


/**
 * Boolean value table cell renderer.
 * 
 * @author Bartosz Firyn (SarXos)
 */
public class BooleanRenderer extends JPanel implements TableCellRenderer, Serializable {

	private static final long serialVersionUID = 8848514762273327844L;

	private JCheckBox checkbox = null;

	public BooleanRenderer() {

		checkbox = new JCheckBox();
		checkbox.setBounds(-3, 0, 200, 15);
		checkbox.setText("");

		setBorder(null);
		setLayout(null);

		add(checkbox);
	}

	@Override
	public Component getTableCellRendererComponent(JTable table, Object value, boolean isSelected, boolean hasFocus, int row, int column) {

		JTable.DropLocation dl = table.getDropLocation();
		if (dl != null && !dl.isInsertRow() && !dl.isInsertColumn() && dl.getRow() == row && dl.getColumn() == column) {
			isSelected = true;
		}

		if (isSelected) {
			setBackground(table.getSelectionBackground());
			setForeground(table.getSelectionForeground());
			checkbox.setBackground(table.getSelectionBackground());
			checkbox.setForeground(table.getSelectionForeground());
		} else {
			setBackground(table.getBackground());
			setForeground(table.getForeground());
			checkbox.setBackground(table.getBackground());
			checkbox.setForeground(table.getForeground());
		}

		boolean selected = Boolean.TRUE.equals(value);

		checkbox.setSelected(selected);
		checkbox.setText(selected ? I18N.TRUE : I18N.FALSE);

		return this;
	}
}
