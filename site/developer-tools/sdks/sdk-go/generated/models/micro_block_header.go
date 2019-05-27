// Code generated by go-swagger; DO NOT EDIT.

package models

// This file was generated by the swagger tool.
// Editing this file might prove futile when you re-run the swagger generate command

import (
	strfmt "github.com/go-openapi/strfmt"

	"github.com/go-openapi/errors"
	"github.com/go-openapi/swag"
	"github.com/go-openapi/validate"
)

// MicroBlockHeader micro block header
// swagger:model MicroBlockHeader
type MicroBlockHeader struct {

	// hash
	// Required: true
	Hash EncodedHash `json:"hash"`

	// height
	// Required: true
	Height *uint64 `json:"height"`

	// "no_fraud" | api encoded Proof of Fraud hash
	// Required: true
	PofHash *string `json:"pof_hash"`

	// prev hash
	// Required: true
	PrevHash EncodedHash `json:"prev_hash"`

	// prev key hash
	// Required: true
	PrevKeyHash EncodedHash `json:"prev_key_hash"`

	// signature
	// Required: true
	Signature EncodedHash `json:"signature"`

	// state hash
	// Required: true
	StateHash EncodedHash `json:"state_hash"`

	// time
	// Required: true
	Time *int64 `json:"time"`

	// txs hash
	// Required: true
	TxsHash EncodedHash `json:"txs_hash"`

	// version
	// Required: true
	Version *uint64 `json:"version"`
}

// Validate validates this micro block header
func (m *MicroBlockHeader) Validate(formats strfmt.Registry) error {
	var res []error

	if err := m.validateHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateHeight(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validatePofHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validatePrevHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validatePrevKeyHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateSignature(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateStateHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateTime(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateTxsHash(formats); err != nil {
		res = append(res, err)
	}

	if err := m.validateVersion(formats); err != nil {
		res = append(res, err)
	}

	if len(res) > 0 {
		return errors.CompositeValidationError(res...)
	}
	return nil
}

func (m *MicroBlockHeader) validateHash(formats strfmt.Registry) error {

	if err := m.Hash.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("hash")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateHeight(formats strfmt.Registry) error {

	if err := validate.Required("height", "body", m.Height); err != nil {
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validatePofHash(formats strfmt.Registry) error {

	if err := validate.Required("pof_hash", "body", m.PofHash); err != nil {
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validatePrevHash(formats strfmt.Registry) error {

	if err := m.PrevHash.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("prev_hash")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validatePrevKeyHash(formats strfmt.Registry) error {

	if err := m.PrevKeyHash.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("prev_key_hash")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateSignature(formats strfmt.Registry) error {

	if err := m.Signature.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("signature")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateStateHash(formats strfmt.Registry) error {

	if err := m.StateHash.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("state_hash")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateTime(formats strfmt.Registry) error {

	if err := validate.Required("time", "body", m.Time); err != nil {
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateTxsHash(formats strfmt.Registry) error {

	if err := m.TxsHash.Validate(formats); err != nil {
		if ve, ok := err.(*errors.Validation); ok {
			return ve.ValidateName("txs_hash")
		}
		return err
	}

	return nil
}

func (m *MicroBlockHeader) validateVersion(formats strfmt.Registry) error {

	if err := validate.Required("version", "body", m.Version); err != nil {
		return err
	}

	return nil
}

// MarshalBinary interface implementation
func (m *MicroBlockHeader) MarshalBinary() ([]byte, error) {
	if m == nil {
		return nil, nil
	}
	return swag.WriteJSON(m)
}

// UnmarshalBinary interface implementation
func (m *MicroBlockHeader) UnmarshalBinary(b []byte) error {
	var res MicroBlockHeader
	if err := swag.ReadJSON(b, &res); err != nil {
		return err
	}
	*m = res
	return nil
}